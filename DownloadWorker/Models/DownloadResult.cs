using System;

namespace DownloadWorker.Models
{
    public class DownloadResult
    {
        public long Size { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Elapsed { get; set; }
        public int ParallelDownloads { get; set; }
    }
}
