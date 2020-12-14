namespace Client.ConsoleApp.Configuration
{
    public class AppConfig
    {
        public string StageBlockUri { get; set; }

        public string CommitBlocksUri { get; set; }

        public string FullPathFileName { get; set; }

        public int MaxThreads { get; set; } = 4;

        public int BlockSize { get; set; } = 1 * 1024 * 1024; // 1Mb

    }
}