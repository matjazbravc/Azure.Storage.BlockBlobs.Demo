using Client.ConsoleApp.Configuration;
using Common.Contacts.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Services.Helpers
{
    /// <summary>
    /// Rest HttpClient helper class
    /// </summary>
    public class RestClientHelper : IRestClientHelper
    {
        private readonly ILogger<RestClientHelper> _logger;
        private readonly AppConfig _config;

        public RestClientHelper(ILoggerFactory loggerFactory, AppConfig config)
        {
            _logger = loggerFactory.CreateLogger<RestClientHelper>();
            _config = config;
        }

        public async Task<HttpStatusCode> StageBlockAsync(string blobName, long blockId, byte[] blockData)
        {
            _logger.LogDebug(nameof(StageBlockAsync));

            using var httpClientHandler = new HttpClientHandler();
            using var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Blob-Name", blobName);
            httpClient.DefaultRequestHeaders.Add("X-Block-Id", blockId.ToString());

            using var content = new ByteArrayContent(blockData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.ContentLength = blockData.Length;
            var response = await httpClient.PostAsync(_config.StageBlockUri, content).ConfigureAwait(false);
            var result = response.StatusCode;
            return result;
        }

        public async Task<HttpStatusCode> CommitBlocksAsync(string blobName, FileMetadata fileMetadata)
        {
            _logger.LogDebug(nameof(CommitBlocksAsync));

            var result = HttpStatusCode.MethodNotAllowed;
            using var httpClientHandler = new HttpClientHandler();
            using var httpClient = new HttpClient(httpClientHandler);
            var url = string.Format(_config.CommitBlocksUri, blobName);
            var content = fileMetadata == null ? null : new StringContent(JsonConvert.SerializeObject(fileMetadata), Encoding.UTF8, "application/json");
            if (content == null)
            {
                return result;
            }
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
            result = response.StatusCode;

            return result;
        }
    }
}