using Client.ConsoleApp.Extensions;
using Client.ConsoleApp.Services.Helpers;
using Common.Contacts.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Services
{
    public class FileUploader : IFileUploader
    {
        private readonly IRestClientHelper _restClientHelper;
        private readonly IUploadStatistics _statistics;
        private readonly ITaskHelper _enumerableHelper;

        public FileUploader(IRestClientHelper restClientHelper, IUploadStatistics statistics, ITaskHelper enumerableHelper)
        {
            _restClientHelper = restClientHelper;
            _statistics = statistics;
            _enumerableHelper = enumerableHelper;
        }

        public async Task<HttpStatusCode> UploadFileAsync(string filePath, int blockSize = 1 * 1024 * 1024, int maxThreads = 4)
        {
            var result = HttpStatusCode.BadRequest;

            FileInfo file = new FileInfo(filePath);
            var blobName = Path.GetFileName(filePath);
            _statistics.Initialize(file.Length);

            try
            {
                // Read all blocks in the file
                var fileBlocks = Enumerable
                    .Range(1, 1 + ((int)(file.Length / blockSize)))
                    .Select(_ => new BlockMetadata(_, blockSize, file.Length))
                    .Where(block => block.Size > 0)
                    .ToList();

                DateTime start = DateTime.UtcNow;

                async Task UploadBlockAsync(BlockMetadata block, IUploadStatistics statistics)
                {
                    // Read file block
                    var blockData = await file.GetFileContentAsync(block.Index, block.Size).ConfigureAwait(false);

                    // Stage block to the container
                    var stageResponseCode = await _restClientHelper.StageBlockAsync(blobName, block.Id, blockData).ConfigureAwait(false);
                    if (stageResponseCode != HttpStatusCode.OK)
                    {
                        throw new HttpRequestException($"StageBlock {block.Id} has failed with code {stageResponseCode}");
                    }

                    // Update upload statistics
                    Program.Logger.LogInformation(statistics.Update(block.Id, blockData.Length, start));
                }

                // Process file blocks in multiple threads (default = 4)
                await _enumerableHelper.ForEachAsync(
                    source: fileBlocks,
                    partitionCount: maxThreads,
                    body: blockMetadata => UploadBlockAsync(blockMetadata, _statistics)).ConfigureAwait(false);

                // Create File metadata info
                FileMetadata fileMetadata = new FileMetadata
                {
                    FileName = blobName,
                    Size = file.Length
                };

                result = await _restClientHelper.CommitBlocksAsync(blobName, fileMetadata).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Program.Logger.LogError(ex.Message);
            }
            return result;
        }
    }
}
