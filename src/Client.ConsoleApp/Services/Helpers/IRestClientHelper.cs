namespace Client.ConsoleApp.Services.Helpers
{
    using Common.Contacts.Models;
    using System.Net;
    using System.Threading.Tasks;

    public interface IRestClientHelper
    {
        Task<HttpStatusCode> StageBlockAsync(string blobName, long blockId, byte[] blockData);

        Task<HttpStatusCode> CommitBlocksAsync(string blobName, FileMetadata fileMetadata);
    }
}