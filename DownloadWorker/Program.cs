using DownloadWorker.Services;
using DownloadWorker.Services.Prompt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DownloadWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IPrompt, Prompt>();
                    services.AddSingleton<IAZCopy, AZCopy>();
                    services.AddSingleton<IDownloadService, AZCopyDownloadService>();
                    //services.AddSingleton<IDownloadService, DownloadService>();
                    services.AddHostedService<Worker>();
                });
    }
}
