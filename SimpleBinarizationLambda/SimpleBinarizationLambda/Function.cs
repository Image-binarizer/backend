using System;
using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using SkiaSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SimpleBinarizationLambda
{
    public class Function
    {
        private const string bucketName = "zpi-client-pictures-bucket";
        private IS3Service _s3Service;

        public Function()
        {
            _s3Service = new S3Service(bucketName: bucketName);
        }

        public async Task<JObject> FunctionHandlerAsync(ImageInfo imageInfo, ILambdaContext context)
        {
            var dowloadedImagePath = await _s3Service.GetImageFromS3($"{imageInfo.UserId}/pictures/original/single/{imageInfo.ImageFileName}", imageInfo.ImageFileName);
            
            var newFileName = imageInfo.ImageFileName.Remove(imageInfo.ImageFileName.Length - 4, 4);
            newFileName = newFileName + "_simpleBinarization.jpg";

            using (var input = File.OpenRead(dowloadedImagePath))
            {
                using (var inputStream = new SKManagedStream(input))
                {
                    using (var original = SKBitmap.Decode(inputStream))
                    {
                        SKBitmap temp = new SKBitmap(original.Info);

                        for (int i = 0; i < original.Width - 1; i++)
                        {
                            for (int j = 0; j < original.Height - 1; j++)
                            {

                                SKColor pixelColor = original.GetPixel(i, j);
                             
                                int ret = (int)(pixelColor.Red * 0.299f + pixelColor.Green * 0.578f + pixelColor.Blue * 0.114f);

                                if (ret > 100)
                                {
                                    temp.SetPixel(i, j, SKColor.FromHsl(0.0f, 0.0f, 100.0f));
                                }
                                else
                                {
                                    temp.SetPixel(i, j, SKColor.FromHsl(0.0f, 0.0f, 0.0f));
                                }
                            }
                        }
                  
                        using (var image = SKImage.FromBitmap(temp))
                        {
                            using (var output = File.OpenWrite($"/tmp/{newFileName}"))
                            {
                                image.Encode(SKEncodedImageFormat.Jpeg, 75)
                                    .SaveTo(output);
                            }
                        }

                    }
                }
            }
            var key = $"{imageInfo.UserId}/pictures/binarization/simple/csharp/{newFileName}";
            var status = await _s3Service.PutImageToS3(key, $"/tmp/{newFileName}");
            return JObject.FromObject(new { StatusCode = status, Body = key });
        }
    }
}
