namespace FileProcessorApi.Services;

public sealed class FileProcessingCounter
{
    private long _filesProcessed;

    public long Increment()
    {
        return Interlocked.Increment(ref _filesProcessed);
    }

    public long FilesProcessed => Interlocked.Read(ref _filesProcessed);
}
