namespace FileProcessorApi.Dtos
{
    public class FileProcessResult
    {
        public Enumerations.FileExtension Extension { get; init; }

        public object? Result { get; init; }

        public long FilesProcessed { get; init; }
    }
}
