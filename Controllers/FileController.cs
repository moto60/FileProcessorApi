using FileProcessorApi.Enumerations;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class FileController : ControllerBase
    {
        [HttpPost("process")]
        public async Task<IActionResult> ProcessFileAsync(IFormFile file) {

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).TrimStart('.');

            if (!Enum.TryParse<FileExtension>(
                    extension,
                    ignoreCase: true,
                    out _))
            {
                return BadRequest("Invalid file extension.");
            }

            return Ok();
        }
    }
}
