namespace App.Controllers
{
    using Azure.Storage.BlockBlobs.Demo.Services;
    using Common.Contacts.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Threading.Tasks;

    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobStorageService _filesStorage;
        private readonly ILogger<BlobController> _logger;

        public BlobController(ILogger<BlobController> logger, IBlobStorageService filesStorage)
        {
            _logger = logger;
            _filesStorage = filesStorage;
        }

        /// <summary>
        /// Stage Blob block
        /// </summary>
        /// <returns></returns>
        [HttpPost("stageblock")]
        public async Task<IActionResult> StageBlockAsync()
        {
            _logger.LogDebug(nameof(StageBlockAsync));

            var blobName = Request.Headers["X-Blob-Name"].ToString();
            var blockId = long.Parse(Request.Headers["X-Block-Id"]);

            using (var stream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0;
                await _filesStorage.StageBlockAsync(blobName, blockId, stream).ConfigureAwait(false);
            }

            return Ok();
        }

        /// <summary>
        /// Commits Blob's blocks
        /// </summary>
        /// <param name="blobName">Blob name</param>
        /// <param name="fileMetadata">File metadata</param>
        /// <returns></returns>
        [HttpPost("commitblocks/{blobName}")]
        public async Task<IActionResult> CommitBlocksAsync(string blobName, [FromBody] FileMetadata fileMetadata)
        {
            _logger.LogDebug(nameof(CommitBlocksAsync));
            
            await _filesStorage.CommitBlocksAsync(blobName, fileMetadata.ToStorage()).ConfigureAwait(false);
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
            var data = await _filesStorage.DownloadAsync(blobName).ConfigureAwait(false);
            return File(data.Stream, "application/force-download", data.Metadata.FileName, enableRangeProcessing: true);
        }
    }
}