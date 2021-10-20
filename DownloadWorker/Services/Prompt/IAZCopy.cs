using DownloadWorker.Models;
using System;
using System.Collections.Generic;

namespace DownloadWorker.Services.Prompt
{
    public interface IAZCopy
    {
        event EventHandler<AZCopyDownloadProgress> DownloadProgress;

        void Download(string blobUrl, string filePath, string token);
        void ResumeJob(string jobId);
        IEnumerable<Job> Jobs();
        void ClearJobs(JobStatus? jobStatus = default);
    }
}
