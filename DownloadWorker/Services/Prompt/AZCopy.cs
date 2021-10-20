using DownloadWorker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DownloadWorker.Services.Prompt
{
    public class AZCopy : IAZCopy
    {
        private readonly IPrompt _prompt;

        public event EventHandler<AZCopyDownloadProgress> DownloadProgress;

        public AZCopy(IPrompt prompt)
        {
            _prompt = prompt;
        }

        public void Download(string blobUrl, string filePath, string token)
        {
            EventHandler<PromptOutputMessageEventArgs> handler = delegate(object sender, PromptOutputMessageEventArgs args)
            {
                DownloadProgress?.Invoke(this, new AZCopyDownloadProgress(args.OutputMessage));
            };

            _prompt.WrittenOutput += handler;
            _prompt.Run($"azcopy copy \"{blobUrl}?{token}\" \"{filePath}\"");
            _prompt.WrittenOutput -= handler;
        }

        public void ResumeJob(string jobId)
        {
        }

        public IEnumerable<Job> Jobs()
        {
            var outputMessages = _prompt.Run("azcopy jobs list");

            var jobIds = outputMessages.ToList().Where(message => message.StartsWith("JobId"));
            var startTimes = outputMessages.ToList().Where(message => message.StartsWith("Start Time"));
            var statusList = outputMessages.ToList().Where(message => message.StartsWith("Status"));
            var commands = outputMessages.ToList().Where(message => message.StartsWith("Command"));

            var jobs = new List<Job>();

            for (int i = 0; i < jobIds.Count(); i++)
            {
                var status = JobStatus.Unknown;
                Enum.TryParse(statusList.ElementAt(i).Replace("Status: ", string.Empty), true, out status);

                jobs.Add(new Job
                {
                    Id = jobIds.ElementAt(i).Replace("JobId: ", string.Empty),
                    StartTime = startTimes.ElementAt(i).Replace("Start Time: ", string.Empty),
                    Status = status,
                    Command = commands.ElementAt(i).Replace("Command: ", string.Empty)
                });
            }

            return jobs;
        }

        public void ClearJobs(JobStatus? jobStatus)
        {
            var status = jobStatus.HasValue ? jobStatus.Value : JobStatus.All;
            _prompt.Run($"azcopy jobs clean --with-status={status.ToString().ToLower()}");
        }
    }
}
