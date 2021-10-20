using System;

namespace DownloadWorker.Models
{
    public class DownloadEnvironment
    {
        public int ParallelDownloads { get; set; }
        public string DestinationFilePath { get; set; }
    }
}
