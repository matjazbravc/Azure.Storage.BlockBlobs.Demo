namespace Azure.Storage.BlockBlobs.Demo.Services
{
    using Common.Contacts.Models;
    using System.IO;
    using System.Threading.Tasks;

    public interface IBlobStorageService
    {
        Task CreateContainerAsync();

        Task StageBlockAsync(string blobName, long blockId, Stream block);

        Task CommitBlocksAsync(string blobName, FileMetadata metadata);

        Task<(Stream Stream, FileMetadata Metadata)> DownloadAsync(string blobName);
    }
}