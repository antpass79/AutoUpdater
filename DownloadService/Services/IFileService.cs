using System.IO;
using System.Threading.Tasks;

namespace DownloadService.Services
{
    public interface IFileService
    {
        string Name { get; }
        string FullName { get; }
        long Length { get; }

        Stream BuildStream();
    }
}
