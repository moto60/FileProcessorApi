using FileProcessorApi.Enumerations;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessorApi.Interfaces
{
    public interface IProcessFile
    {
        Task<object> ProcessAsync(Stream input, CancellationToken cancellationToken);
    }

    public interface IProcessFileFactory
    {
        IProcessFile Create(FileExtension fileExtension);
    }
}
