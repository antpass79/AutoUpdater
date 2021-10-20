using DownloadWorker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDownloadService _downloadService;

        public Worker(
            ILogger<Worker> logger,
            IDownloadService downloadService)
        {
            _logger = logger;
            _downloadService = downloadService;

            _downloadService.Progress += (sender, progress) =>
            {
                Console.WriteLine($"Chunk {progress.Chunk} of {progress.ParallelDownloads}");
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Press any key to download");
                Console.ReadKey();
                Console.WriteLine($"Download starts...");

                var result = await _downloadService.DownloadAsync();

                Console.WriteLine($"Location: {result.FilePath}");
                Console.WriteLine($"Size: {result.Size} bytes");
                Console.WriteLine($"Elapsed: {result.Elapsed}");
                Console.WriteLine($"Parallel: {result.ParallelDownloads}");

                Console.WriteLine();
            }
        }
    }
}
