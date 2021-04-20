using System.IO;
using System.Threading.Tasks;
using Azure.Storage.BlockBlobs.Demo.Services;
using Common.Contacts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Azure.Storage.BlockBlobs.Demo.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<BlobController> _logger;

        public BlobController(ILogger<BlobController> logger, IBlobStorageService blobStorage)
        {
            _logger = logger;
            _blobStorage = blobStorage;
        }

        /// <summary>
        /// Stage Blob block
        /// </summary>
        /// <returns></returns>
        [HttpPost("stageBlock")]
        public async Task<IActionResult> StageBlockAsync()
        {
            _logger.LogDebug(nameof(StageBlockAsync));

            var blobName = Request.Headers["X-Blob-Name"].ToString();
            var blockId = long.Parse(Request.Headers["X-Block-Id"]);

            await using (var stream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0;
                await _blobStorage.StageBlockAsync(blobName, blockId, stream).ConfigureAwait(false);
            }

            return Ok();
        }

        /// <summary>
        /// Commits Blob's blocks
        /// </summary>
        /// <param name="blobName">Blob name</param>
        /// <param name="fileMetadata">File metadata</param>
        /// <returns></returns>
        [HttpPost("commitBlocks/{blobName}")]
        public async Task<IActionResult> CommitBlocksAsync(string blobName, [FromBody] FileMetadata fileMetadata)
        {
            _logger.LogDebug(nameof(CommitBlocksAsync));
            
            await _blobStorage.CommitBlocksAsync(blobName, fileMetadata.ToStorage()).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Downloads Blob
        /// </summary>
        /// <param name="blobName">Blob name</param>
        /// <returns></returns>
        [HttpGet("{blobName}")]
        public async Task<IActionResult> DownloadBlobAsync(string blobName)
        {
            _logger.LogDebug(nameof(DownloadBlobAsync));
            var (stream, metadata) = await _blobStorage.DownloadAsync(blobName).ConfigureAwait(false);
            return File(stream, "application/force-download", metadata.FileName, enableRangeProcessing: true);
        }
    }
}