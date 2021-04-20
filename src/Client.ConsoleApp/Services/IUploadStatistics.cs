using System;

namespace Client.ConsoleApp.Services
{
    public interface IUploadStatistics
    {
        void Initialize(long fileSize);

        string Update(int blockId, long blockBytes, DateTime start);
    }
}