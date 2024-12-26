using Azure.Storage.Blobs;
using System.Text.Json;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace GillingeClassicCars.Handlers
{
    public class MediaUploadedHandler : INotificationHandler<MediaSavedNotification>
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=gillingeblob;AccountKey=5VJ0yf98jwmVMm2m89NfA+SNOT+XYrtHm299BYIUxB+a8n7TSGBa4Lc2uJrhjGZNZXq35XgB/g4P+AStxxUqlw==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "umbraco";

        public void Handle(MediaSavedNotification notification)
        {
            foreach (var media in notification.SavedEntities)
            {
                if (media.ContentType.Alias == "Image") // Kontrollera att det är en bild
                {
                    // Hämta AltText från Umbraco
                    var altText = media.GetValue<string>("altText");
                    var filePathJson = media.GetValue<string>("umbracoFile");

                    // Parsar JSON för att hämta "src"-värdet
                    string blobName;
                    try
                    {
                        var filePathData = JsonDocument.Parse(filePathJson);
                        blobName = filePathData.RootElement.GetProperty("src").GetString()?.TrimStart('/');
                    }
                    catch (Exception ex)
                    {
                        // Logga om JSON inte kan parsas
                        Console.WriteLine($"Error parsing filePath JSON: {ex.Message}");
                        continue;
                    }

                    if (string.IsNullOrEmpty(blobName))
                    {
                        Console.WriteLine("BlobName could not be extracted from JSON.");
                        continue;
                    }

                    // Lägg till metadata i Azure Blob Storage
                    var blobClient = new BlobServiceClient(_connectionString)
                        .GetBlobContainerClient(_containerName)
                        .GetBlobClient(blobName);

                    var metadata = new Dictionary<string, string>
                {
                    { "altText", altText }
                };

                    // Kontrollera om blobben existerar och sätt metadata
                    //if (blobClient.Exists())
                    //{
                    //    blobClient.SetMetadata(metadata);
                    //}
                }
            }
        }
    }
}
