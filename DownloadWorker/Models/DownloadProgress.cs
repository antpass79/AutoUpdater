namespace DownloadWorker.Models
{
    public class DownloadProgress
    {
        public DownloadProgress(int parallelDownloads, int chunk)
        {
            ParallelDownloads = parallelDownloads;
            Chunk = chunk;
        }

        public int ParallelDownloads { get; }
        public int Chunk { get; }
    }
}
