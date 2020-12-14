namespace Client.ConsoleApp.Services
{
    using System;

    public interface IUploadStatistics
    {
        void Initialize(long fileSize);

        string Update(int blockId, long blockBytes, DateTime start);
    }
}