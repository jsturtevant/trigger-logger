using System.Text.Json;
using Azure.Storage.Blobs;

public interface Outputers
{
    Task Run(string filename);
}

public class AzureStorageConfig
{
    public string connectionstring { get; set; }
    public string containerName { get; set; }
}

public class AzureStorageOutputer : Outputers
{
    private AzureStorageConfig config;

    public AzureStorageOutputer(JsonElement config)
    {
        this.config = JsonSerializer.Deserialize<AzureStorageConfig>(config);
    }

    public async Task Run(string filename)
    {
        Console.WriteLine($"uploading to azure storage");
        BlobContainerClient container = new BlobContainerClient(this.config.connectionstring, this.config.containerName);
        await container.CreateIfNotExistsAsync();

        string blobname = Path.GetFileName(filename);
        BlobClient blob = container.GetBlobClient(blobname);

        await blob.UploadAsync(filename);
        
        Console.WriteLine($"uploaded to azure storage at {blob.Uri.ToString()}");
    }
}