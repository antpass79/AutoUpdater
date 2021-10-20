namespace DownloadWorker.Models
{
    public class AZCopyDownloadProgress
    {
        public AZCopyDownloadProgress(string percentange)
        {
            Percentange = percentange;
        }

        public string Percentange { get; }
    }
}
