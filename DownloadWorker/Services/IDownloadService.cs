using DownloadWorker.Models;
using System;
using System.Threading.Tasks;

namespace DownloadWorker.Services
{
    public interface IDownloadService
    {
        event EventHandler<DownloadProgress> Progress;

        Task<DownloadResult> DownloadAsync();
    }
}
