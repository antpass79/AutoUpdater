using DownloadWorker.Models;
using DownloadWorker.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DownloadWorker.Services
{
    public class DownloadService : IDownloadService
    {
        #region Data Members

        private readonly IConfiguration _configuration;

        #endregion

        #region Events

        public event EventHandler<DownloadProgress> Progress;

        #endregion

        #region Constructors

        public DownloadService(IConfiguration configuration)
        {
            _configuration = configuration;

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 1000;
        }

        #endregion

        #region Public Functions

        async public Task<DownloadResult> DownloadAsync()
        {
            var configuration = new
            {
                Url = _configuration.GetValue<string>("Download:Url"),
                OutputFolder = _configuration.GetValue<string>("Download:OutputFolder"),
                ParallelDownloads = _configuration.GetValue<int>("Download:ParallelDownloads"),
            };

            return await DownloadAsync(configuration.Url, configuration.OutputFolder, configuration.ParallelDownloads);
        }

        #endregion

        #region Private Functions

        async Task<DownloadResult> DownloadAsync(string fileUrl, string destinationFolderPath, int parallelDownloads = 0, bool validateSSL = false)
        {
            using var watcher = new Watcher("DownloadAsync");

            var environment = PrepareEnvironment(destinationFolderPath, validateSSL, fileUrl, parallelDownloads);
            var responseLength = AskFileSize(fileUrl);
            var readRanges = CalculateRange(parallelDownloads, (int)responseLength);
            var chunks = DownloadChunks(readRanges, parallelDownloads, fileUrl);
            MergeChunks(chunks, environment.DestinationFilePath);

            return await Task.FromResult(new DownloadResult
            {
                FilePath = environment.DestinationFilePath,
                Size = responseLength,
                ParallelDownloads = chunks.Count,
                Elapsed = watcher.Elapsed
            });
        }

        private DownloadEnvironment PrepareEnvironment(string destinationFolderPath, bool validateSSL, string fileUrl, int parallelDownloads)
        {
            using var watcher = new Watcher("PrepareEnvironment");

            if (!validateSSL)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            var uri = new Uri(fileUrl);
            var destinationFilePath = Path.Combine(destinationFolderPath, uri.Segments.Last());
            if (!Directory.Exists(destinationFolderPath))
                Directory.CreateDirectory(destinationFolderPath);

            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }

            return new DownloadEnvironment
            {
                ParallelDownloads = parallelDownloads <= 0 ? Environment.ProcessorCount : parallelDownloads,
                DestinationFilePath = destinationFilePath
            };
        }

        private long AskFileSize(string fileUrl)
        {
            using var watcher = new Watcher("AskFileSize");

            WebRequest webRequest = HttpWebRequest.Create(fileUrl);
            webRequest.Method = "HEAD";
            long responseLength;
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                responseLength = long.Parse(webResponse.Headers.Get("Content-Length"));
                return responseLength;
            }
        }

        private IEnumerable<Range> CalculateRange(int parallelDownloads, int responseLength)
        {
            using var watcher = new Watcher("CalculateRange");

            var readRanges = new List<Range>();
            for (int chunk = 0; chunk < parallelDownloads - 1; chunk++)
            {
                readRanges.Add(new Range(
                    new Index(chunk * (responseLength / parallelDownloads)),
                    new Index(((chunk + 1) * (responseLength / parallelDownloads)) - 1)
                ));
            }

            readRanges.Add(new Range(
                new Index(readRanges.Any() ? readRanges.Last().End.Value + 1 : 0),
                new Index((int)responseLength - 1)
            ));

            return readRanges;
        }

        private ConcurrentDictionary<int, string> DownloadChunks(
            IEnumerable<Range> readRanges,
            int parallelDownloads,
            string fileUrl)
        {
            using var watcher = new Watcher("DownloadChunks");

            var tempFilesDictionary = new ConcurrentDictionary<int, string>();

            long chunks = 0;
            Parallel.ForEach(
                readRanges,
                new ParallelOptions() { MaxDegreeOfParallelism = parallelDownloads },
                (readRange, state, chunk) =>
                {
                    chunks = chunk;
                    HttpWebRequest httpWebRequest = WebRequest.Create(fileUrl) as HttpWebRequest;
                    httpWebRequest.Method = "GET";
                    httpWebRequest.AddRange(readRange.Start.Value, readRange.End.Value);

                    using var httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                    var tempFilePath = Path.Combine("Temp", Guid.NewGuid().ToString());

                    using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write);
                    httpWebResponse.GetResponseStream().CopyTo(fileStream);
                    tempFilesDictionary.TryAdd((int)chunk, tempFilePath);

                    Progress?.Invoke(this, new DownloadProgress(parallelDownloads, (int)chunk + 1));
                });

            return tempFilesDictionary;
        }

        private void MergeChunks(ConcurrentDictionary<int, string> chunks, string destinationFilePath)
        {
            using var watcher = new Watcher("MergeChunks");

            using var destinationStream = File.OpenWrite(destinationFilePath);
            var orderedChunks = chunks.OrderBy(b => b.Key);
            foreach (var chunk in orderedChunks)
            {
                using (var inputStream = File.OpenRead(chunk.Value))
                    inputStream.CopyTo(destinationStream);

                File.Delete(chunk.Value);
            }
        }

        #endregion
    }
}
