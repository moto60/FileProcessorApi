using FileProcessorApi.Enumerations;
using FileProcessorApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class FileController : ControllerBase
    {
        private readonly IProcessFileFactory _processFileFactory;

        public FileController(IProcessFileFactory processFileFactory)
        {
            _processFileFactory = processFileFactory;
        }

        [HttpPost("process")]
        public IActionResult ProcessFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).TrimStart('.');

            if (!Enum.TryParse<FileExtension>(
                    extension,
                    ignoreCase: true,
                    out var fileExtension)
                || !Enum.IsDefined(fileExtension))
            {
                return BadRequest("Invalid file extension.");
            }

            _processFileFactory.Create(fileExtension);

            return Ok();
        }
    }
}
