using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace DataAccess.SupportServices
{
    public interface IStorageService
    {
        Task CreateBucketAsync(string bucketName);
        Task<string> UploadFileAsync(string bucketName, IFormFile file);
        Task<string> UploadFileAsync(string bucketName, string fileName, Stream fileStream, string contentType = null);
        Task<Stream> DownloadFileAsync(string bucketName, string fileName);
        Task DeleteFileAsync(string bucketName, string fileName);
        Task<List<string>> ListObjectsAsync(string bucketName);
        Task<bool> FileExistsAsync(string bucketName, string fileName);
    }

    public class StorageService : IStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<StorageService> _logger;

        public StorageService(IMinioClient minioClient, ILogger<StorageService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                bool bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName));

                if (!bucketExists)
                {
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(bucketName));

                    _logger.LogInformation($"Bucket '{bucketName}' created successfully");
                }
            }
            catch (MinioException ex)
            {
                _logger.LogError($"Error creating bucket: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(string bucketName, IFormFile file)
        {
            await CreateBucketAsync(bucketName);

            string fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using (var fileStream = file.OpenReadStream())
            {
                return await UploadFileAsync(bucketName, fileName, fileStream, file.ContentType);
            }
        }

        public async Task<string> UploadFileAsync(
            string bucketName,
            string fileName,
            Stream fileStream,
            string contentType = null)
        {
            try
            {
                await CreateBucketAsync(bucketName);

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType ?? "application/octet-stream");

                await _minioClient.PutObjectAsync(putObjectArgs);

                _logger.LogInformation($"File '{fileName}' uploaded to bucket '{bucketName}'");

                return fileName;
            }
            catch (MinioException ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string bucketName, string fileName)
        {
            try
            {
                var memoryStream = new MemoryStream();

                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                    });

                await _minioClient.GetObjectAsync(getObjectArgs);

                _logger.LogInformation($"File '{fileName}' downloaded from bucket '{bucketName}'");

                return memoryStream;
            }
            catch (MinioException ex)
            {
                _logger.LogError($"Error downloading file: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string bucketName, string fileName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);

                _logger.LogInformation($"File '{fileName}' deleted from bucket '{bucketName}'");
            }
            catch (MinioException ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> ListObjectsAsync(string bucketName)
        {
            var objects = new List<string>();

            try
            {
                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithRecursive(true);

                await foreach (var item in _minioClient.ListObjectsEnumAsync(listArgs))
                {
                    objects.Add(item.Key);
                }

                return objects;
            }
            catch (MinioException ex)
            {
                _logger.LogError($"Error listing objects: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string bucketName, string fileName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (MinioException)
            {
                return false;
            }
        }
    }
}
