using System.Net;
using System.Threading.Tasks;

namespace SimpleBinarizationLambda
{
    public interface IS3Service
    {
        Task<HttpStatusCode> PutImageToS3(string key, string filePath);
        Task<string> GetImageFromS3(string key, string fileName);
    }
}
