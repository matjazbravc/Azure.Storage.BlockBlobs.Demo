using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.BlockBlobs.Demo.Configuration;
using Common.Contacts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Storage.BlockBlobs.Demo.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageConfig _blobStorageConfig;

        public BlobStorageService(BlobStorageConfig blobStorageConfig)
        {
            _blobStorageConfig = blobStorageConfig;
        }

        public async Task CommitBlocksAsync(string blobName, FileMetadata metadata)
        {
            var client = CreateBlockClient(blobName);
            var blockList = await client.GetBlockListAsync();
            var blobBlockIds = blockList.Value.UncommittedBlocks.Select(item => item.Name);
            var commitOptions = new CommitBlockListOptions
            {
                Metadata = metadata.ToStorageMetadata()
            };
            var result = client.CommitBlockListAsync(OrderBlobBlockIds(blobBlockIds), commitOptions);
            await result;
        }

        public async Task CreateContainerAsync()
        {
            var serviceClient = new BlobServiceClient(_blobStorageConfig.ConnectionString);
            var blobClient = serviceClient.GetBlobContainerClient(_blobStorageConfig.ContainerName);
            await blobClient.CreateIfNotExistsAsync();
        }

        public async Task<(Stream Stream, FileMetadata Metadata)> DownloadAsync(string blobName)
        {
            var client = CreateBlockClient(blobName);
            var blob = await client.DownloadAsync();
            var properties = await client.GetPropertiesAsync();

            var result = (Stream: blob.Value.Content, Metadata: FileMetadata.FromStorageMetadata(properties.Value.Metadata));
            return result;
        }

        public async Task StageBlockAsync(string blobName, long blockId, Stream blockData)
        {
            var client = CreateBlockClient(blobName);
            using var md5 = MD5.Create();
            var contentHash = await md5.ComputeHashAsync(blockData);
            blockData.Seek(0, SeekOrigin.Begin);
            await client.StageBlockAsync(GetBlobBlockId(blockId), blockData, contentHash);
        }

        private BlockBlobClient CreateBlockClient(string blobName)
        {
            return new(_blobStorageConfig.ConnectionString, _blobStorageConfig.ContainerName, blobName);
        }

        private static string GetBlobBlockId(long blockId)
        {
            var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId.ToString("d20")));
            return result;
        }

        private static IEnumerable<string> OrderBlobBlockIds(IEnumerable<string> blobBlockIds)
        {
            return blobBlockIds.Select(Convert.FromBase64String)
                .Select(Encoding.UTF8.GetString)
                .Select(long.Parse)
                .OrderBy(_ => _)
                .Select(GetBlobBlockId)
                .ToArray();
        }
    }
}