using System;

namespace Client.ConsoleApp.Services
{
    public class UploadStatistics : IUploadStatistics
    {
        private const long GB = MB * 1024;
        private const int KB = 1024;
        private const int MB = KB * 1024;
        private readonly DateTime _initialStartTime = DateTime.UtcNow;
        private readonly object _lock = new();
        private long _completed;
        private long _totalBytes;

        public void Initialize(long fileSize)
        {
            _totalBytes = fileSize;
        }

        public string Update(int blockId, long blockBytes, DateTime start)
        {
            lock (_lock)
            {
                _completed += blockBytes;
            }

            var kbPerSec = (blockBytes / (DateTime.UtcNow.Subtract(start).TotalSeconds * KB));
            var mbPerMin = (blockBytes / (DateTime.UtcNow.Subtract(start).TotalMinutes * MB));

            return $"Uploaded block {TotalProgress(blockId, _totalBytes)} ({Progress(_completed, _totalBytes)}) with {kbPerSec:F0} kB/sec ({mbPerMin:F1} MB/min), {EstimatedEndTime()}";
        }

        private static string Progress(long current, long total)
        {
            return $"{((int)(100.0 * current / total))}%";
        }

        private static string TotalProgress(int blockId, long total)
        {
            switch (total)
            {
                case < KB:
                    {
                        return $"{blockId} of {total} bytes";
                    }
                case < 10 * MB:
                    {
                        return $"{blockId} of {total / KB} KB";
                    }
                case < 10 * GB:
                    {
                        return $"{blockId} of {total/MB} MB";
                    }
                default:
                    {
                        return $"{blockId} of {total / GB} GB";
                    }
            }
        }

        private string EstimatedEndTime()
        {
            var now = DateTime.UtcNow;
            var elapsedSeconds = now.Subtract(_initialStartTime).TotalSeconds;
            var progress = _completed / ((double)_totalBytes);

            if (_completed == 0)
            {
                return "Unknown time";
            }

            var remainingSeconds = elapsedSeconds * (1 - progress) / progress;
            var remaining = TimeSpan.FromSeconds(remainingSeconds);

            return $"time remaining {remaining:hh\\:mm\\:ss}, (expected end time {now.ToLocalTime().Add(remaining)})";
        }
    }
}