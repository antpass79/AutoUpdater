using System.IO;
using System.Threading.Tasks;

namespace DownloadService.Services
{
    public class FileService : IFileService
    {
        static object _lock = new object();

        FileInfo _fileInfo;
        Stream _stream = new MemoryStream();

        public FileService(string filePath)
        {
            _fileInfo = new FileInfo(filePath);
            using var fileStream = new FileStream(_fileInfo.FullName, FileMode.Open);
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.CopyTo(_stream);
        }

        public string Name => _fileInfo.Name;
        public string FullName => _fileInfo.FullName;

        public long Length => _fileInfo.Length;

        public Stream BuildStream()
        {
            lock (_lock)
            {
                var memoryStream = new MemoryStream();
                _stream.Seek(0, SeekOrigin.Begin);
                _stream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }
    }
}
