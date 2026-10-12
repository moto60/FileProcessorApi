using FileProcessorApi.Enumerations;
using FileProcessorApi.Interfaces;
using FileProcessorApi.Services;

namespace FileProcessorApi.Factories;

public sealed class FileProcessFactory : IProcessFileFactory
{
    private readonly CsvProcessFileService _csvProcessFileService;
    private readonly JsonProcessFileService _jsonProcessFileService;

    public FileProcessFactory(
        CsvProcessFileService csvProcessFileService,
        JsonProcessFileService jsonProcessFileService)
    {
        _csvProcessFileService = csvProcessFileService;
        _jsonProcessFileService = jsonProcessFileService;
    }

    public IProcessFile Create(FileExtension fileExtension)
    {
        return fileExtension switch
        {
            FileExtension.Csv => _csvProcessFileService,
            FileExtension.Json => _jsonProcessFileService,
            _ => throw new ArgumentOutOfRangeException(
                nameof(fileExtension),
                fileExtension,
                "Unsupported file extension.")
        };
    }
}
