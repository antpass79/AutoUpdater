using System;

namespace DownloadWorker.Services.Prompt
{
    public class Job
    {
        public string Id { get; set; }
        public string StartTime { get; set; }
        public string Command { get; set; }
        public JobStatus Status { get; set; }
    }
}