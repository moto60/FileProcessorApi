using FileProcessorApi.Enumerations;
using FileProcessorApi.Dtos;
using FileProcessorApi.Interfaces;
using FileProcessorApi.Middleware;
using FileProcessorApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;

namespace FileProcessorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class FileController : ControllerBase
    {
        private readonly IProcessFileFactory _processFileFactory;
        private readonly FileProcessingCounter _fileProcessingCounter;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IProcessFileFactory processFileFactory,
            FileProcessingCounter fileProcessingCounter,
            ILogger<FileController> logger)
        {
            _processFileFactory = processFileFactory;
            _fileProcessingCounter = fileProcessingCounter;
            _logger = logger;
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
                var stopwatch = Stopwatch.StartNew();
                await using var stream = file.OpenReadStream();
                var result = await processor.ProcessAsync(stream, cancellationToken);
                stopwatch.Stop();
                var filesProcessed = _fileProcessingCounter.Increment();

                _logger.LogInformation(
                    "Processed file {FileName} in {ProcessingTimeMs} ms. Total files processed: {FilesProcessed}.",
                    file.FileName,
                    stopwatch.Elapsed.TotalMilliseconds,
                    filesProcessed);

                return Ok(new FileProcessResult
                {
                    Extension = fileExtension,
                    Result = result,
                    FilesProcessed = filesProcessed
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
