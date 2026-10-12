using FileProcessorApi.Enumerations;
using FileProcessorApi.Dtos;
using FileProcessorApi.Interfaces;
using FileProcessorApi.Middleware;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        [ApiKeyRequired]
        public async Task<IActionResult> ProcessFile(
            IFormFile file,
            CancellationToken cancellationToken)
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

            var processor = _processFileFactory.Create(fileExtension);

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await processor.ProcessAsync(stream, cancellationToken);

                return Ok(new FileProcessResult
                {
                    Extension = fileExtension,
                    Result = result
                });
            }
            catch (InvalidDataException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (JsonException exception)
            {
                return BadRequest($"Invalid JSON: {exception.Message}");
            }
        }
    }
}
