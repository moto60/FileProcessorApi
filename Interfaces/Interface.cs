using FileProcessorApi.Enumerations;

namespace FileProcessorApi.Interfaces
{
    public interface IProcessFile
    {
    }

    public interface IProcessFileFactory
    {
        IProcessFile Create(FileExtension fileExtension);
    }
}
