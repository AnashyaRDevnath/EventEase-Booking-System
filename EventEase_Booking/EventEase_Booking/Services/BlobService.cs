using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase_Booking.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            // Reads the values from your appsettings.json file
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _containerName = configuration["BlobContainerName"] ?? "venue-images";
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Uploads an image file and returns its public URL
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Get or create the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate a unique filename to avoid overwriting existing blobs
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        // Deletes a blob by its URL
        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Silently handle deletion failures - blob may not exist
            }
        }
    }
}
