using Minio;
using Minio.DataModel.Args;
using ScopeSkyCafeteria.Services.Interfaces;

namespace ScopeSkyCafeteria.Services.Implementations;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient minioClient;
    private readonly IConfiguration configuration;

    public MinioFileStorageService(
        IMinioClient minioClient,
        IConfiguration configuration)
    {
        this.minioClient = minioClient;
        this.configuration = configuration;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var bucketName = configuration["MINIO_BUCKET"]
            ?? throw new InvalidOperationException("MINIO_BUCKET is missing.");

        var endpoint = configuration["MINIO_ENDPOINT"]
            ?? throw new InvalidOperationException("MINIO_ENDPOINT is missing.");

        Console.WriteLine($"MINIO ENDPOINT: {endpoint}");
        Console.WriteLine($"MINIO BUCKET: {bucketName}");
        Console.WriteLine($"FILE NAME: {file.FileName}");
        Console.WriteLine($"FILE SIZE: {file.Length}");

        var extension = Path.GetExtension(file.FileName);

        var objectName =
            $"{folder.Trim('/')}/{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();

        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType);

        Console.WriteLine($"UPLOADING OBJECT: {objectName}");

        await minioClient.PutObjectAsync(args);

      

        return objectName;
    }
    public async Task DeleteAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        var bucketName = configuration["MINIO_BUCKET"]
            ?? throw new InvalidOperationException("MINIO_BUCKET is missing.");

        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(filePath);
        
        await minioClient.RemoveObjectAsync(args);
    }
}