using System.Net;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Services
{
    public interface IFileUploader
    {
        Task<HttpStatusCode> UploadFileAsync(string filePath, int blockSize = 1 * 1024 * 1024, int numParallelTasks = 4);
    }
}