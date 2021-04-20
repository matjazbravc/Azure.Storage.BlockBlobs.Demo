using Common.Contacts.Models;
using System.Net;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Services.Helpers
{
    public interface IRestClientHelper
    {
        Task<HttpStatusCode> StageBlockAsync(string blobName, long blockId, byte[] blockData);

        Task<HttpStatusCode> CommitBlocksAsync(string blobName, FileMetadata fileMetadata);
    }
}