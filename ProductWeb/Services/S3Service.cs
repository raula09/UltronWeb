using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration configuration)
    {
        var accessKey = configuration["AWS:AccessKeyId"];
        var secretKey = configuration["AWS:SecretAccessKey"];
        var region = configuration["AWS:Region"];
        _bucketName = configuration["AWS:BucketName"];

        var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var awsRegion = RegionEndpoint.GetBySystemName(region);
        _s3Client = new AmazonS3Client(awsCredentials, awsRegion);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var fileName = System.Guid.NewGuid() + Path.GetExtension(file.FileName);

        using var stream = file.OpenReadStream();
        var fileTransferUtility = new TransferUtility(_s3Client);

        await fileTransferUtility.UploadAsync(stream, _bucketName, fileName);

        // Return public URL
        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
    }
}
