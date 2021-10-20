using DownloadWorker.Models;
using DownloadWorker.Services.Prompt;
using DownloadWorker.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadWorker.Services
{
    public class AZCopyDownloadService : IDownloadService
    {
        #region Data Members

        private readonly IAZCopy _azCopy;
        private readonly IConfiguration _configuration;

        #endregion

        #region Events

        public event EventHandler<DownloadProgress> Progress;

        #endregion

        #region Constructors

        public AZCopyDownloadService(
            IConfiguration configuration,
            IAZCopy azCopy)
        {
            _configuration = configuration;
            _azCopy = azCopy;
        }

        #endregion

        #region Public Functions

        async public Task<DownloadResult> DownloadAsync()
        {
            var configuration = new
            {
                Url = _configuration.GetValue<string>("Download:Url"),
                OutputFolder = _configuration.GetValue<string>("Download:OutputFolder"),
                Token = _configuration.GetValue<string>("Download:AZCopy:Token"),
                FileName = _configuration.GetValue<string>("Download:AZCopy:FileName")
            };

            EventHandler<AZCopyDownloadProgress> handler = delegate (object sender, AZCopyDownloadProgress args)
            {
                Console.WriteLine(args.Percentange);
            };

            _azCopy.DownloadProgress += handler;
            _azCopy.Download(configuration.Url, $"{configuration.OutputFolder}\\{configuration.FileName}", configuration.Token);
            _azCopy.DownloadProgress -= handler;

            return await Task.FromResult(new DownloadResult());
        }

        #endregion

        #region Private Functions

        #endregion
    }
}
