using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SimpleBinarizationLambda
{
    public class S3Service : IS3Service
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public S3Service(string bucketName)
        {
            _s3Client = new AmazonS3Client();
            _bucketName = bucketName;
        }

        public async Task<string> GetImageFromS3(string key, string fileName)
        {
            await _s3Client.DownloadToFilePathAsync(bucketName: _bucketName, key, $"/tmp/{fileName}", null);
            return $"/tmp/{fileName}";
        }

        public async Task<HttpStatusCode> PutImageToS3(string key, string filePath)
        {
            HttpStatusCode resultCode = HttpStatusCode.InternalServerError;

            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
            };

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                request.InputStream = stream;
                try
                {
                    PutObjectResponse response = await _s3Client.PutObjectAsync(request);
                    resultCode = response.HttpStatusCode;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return resultCode;
        }
    }
}
