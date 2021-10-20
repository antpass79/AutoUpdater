using DownloadService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DownloadService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DownloadController : Controller
	{
		readonly ILogger<DownloadController> _logger;
		readonly IFileService _fileService;

		public DownloadController(
			ILogger<DownloadController> logger,
			IFileService fileService)
        {
			_logger = logger;
			_fileService = fileService;
		}

		[HttpHead]
		[HttpGet]
		async public Task<IActionResult> Download()
		{
			// File to download
			if (Request.Method == "HEAD")
				SetResponseHeaders(_fileService);
			else
				await DownloadContentAsync(_fileService);

			return new EmptyResult();
		}

		async private Task DownloadContentAsync(IFileService fileService)
		{
			Response.Headers.Add("Accept-Ranges", "bytes");
			Response.Headers.Add("Content-Disposition", $"attachment; filename={fileService.Name}");
			//
			long start = 1, end = fileService.Length, length;
			byte[] buffer;
			if (Request.Headers["Range"].Count > 0 && Request.Headers["Range"][0] != null)
			{
				string[] range = Request.Headers["Range"][0].Split(new[] { '=', '-' });
				if (range.Length > 1) long.TryParse(range[1], out start);
				if (start <= 0 || start > fileService.Length) start = 1;
				if (range.Length > 2) long.TryParse(range[2], out end);
				if (end > fileService.Length || end <= 0) end = fileService.Length;
			}
			else
			{
				// If no Range header found, send the entire file content
				start = 1;
				end = fileService.Length;
			}
			length = end - start + 1;
			buffer = new byte[length];
			Response.ContentLength = length;
			if (Request.Headers["Range"].Count > 0)
			{
				Response.StatusCode = 206;
				Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileService.Length}");
			}
			else
			{
				Response.StatusCode = 200;
			}
			//using var fileStream = new FileStream(fileService.FullName, FileMode.Open);
			using var stream = fileService.BuildStream();
			if (stream.CanSeek)
			{
				stream.Seek(start - 1, SeekOrigin.Begin);
				
				await stream.ReadAsync(buffer, 0, (int)length);
				await Response.Body.WriteAsync(buffer, 0, buffer.Length);
			}
			else
			{
				//Response.Body.Write($"Can't return content range.");
			}
		}

		private void SetResponseHeaders(IFileService fileService)
		{
			Response.StatusCode = 200;
			Response.Headers.Add("Accept-Ranges", "bytes");
			Response.ContentType = "application/octet-stream"; // Change MIME based on file type
			Response.Headers.Add("Content-Disposition", $"attachment; filename={fileService.Name}");
			Response.ContentLength = fileService.Length;
		}
	}
}