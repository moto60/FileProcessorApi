using System.Text;
using FileProcessorApi.Controllers;
using FileProcessorApi.Dtos;
using FileProcessorApi.Enumerations;
using FileProcessorApi.Interfaces;
using FileProcessorApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileProcessorApi.Tests;

public sealed class FileControllerTests
{
    [Fact]
    public async Task ProcessFile_IncrementsCounterForSuccessfullyProcessedFiles()
    {
        var counter = new FileProcessingCounter();
        var controller = new FileController(
            new TestProcessFileFactory(),
            counter,
            NullLogger<FileController>.Instance);

        var firstResponse = await controller.ProcessFile(CreateFile("first.csv"), CancellationToken.None);
        var secondResponse = await controller.ProcessFile(CreateFile("second.csv"), CancellationToken.None);

        var firstResult = Assert.IsType<FileProcessResult>(
            Assert.IsType<OkObjectResult>(firstResponse).Value);
        var secondResult = Assert.IsType<FileProcessResult>(
            Assert.IsType<OkObjectResult>(secondResponse).Value);

        Assert.Equal(1, firstResult.FilesProcessed);
        Assert.Equal(2, secondResult.FilesProcessed);
        Assert.Equal(2, counter.FilesProcessed);
    }

    [Fact]
    public async Task ProcessFile_DoesNotIncrementCounterWhenFileIsInvalid()
    {
        var counter = new FileProcessingCounter();
        var controller = new FileController(
            new TestProcessFileFactory(),
            counter,
            NullLogger<FileController>.Instance);

        var response = await controller.ProcessFile(CreateFile("report.txt"), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(0, counter.FilesProcessed);
    }

    private static IFormFile CreateFile(string fileName)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        return new FormFile(stream, 0, stream.Length, "file", fileName);
    }

    private sealed class TestProcessFileFactory : IProcessFileFactory
    {
        public IProcessFile Create(FileExtension fileExtension) => new TestProcessFile();
    }

    private sealed class TestProcessFile : IProcessFile
    {
        public Task<object> ProcessAsync(Stream input, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>("processed");
        }
    }
}
