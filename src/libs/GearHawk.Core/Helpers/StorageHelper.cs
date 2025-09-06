using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearHawk.Core.Helpers
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; } = "";
        public string AccountKey { get; set; } = "";
        public string ImageContainer { get; set; } = "";
        public string ThumbnailContainer { get; set; } = "";
        public Uri AccountUri { get; set; }


    }

    public static class StorageHelper
    {
        public static AzureStorageConfig AzureStorageConfigInstance()
        {
            AzureStorageConfig storageConfig = new AzureStorageConfig();
            storageConfig.AccountKey = "Qh88kupOWr7yXUq2RPtBntfd1Mi2wJ+VAPxqzkr6PJRyiShnSQxNjuCDNQulSVAmozhuc/mTeZ3p+yxdjI/23Q==";
            storageConfig.AccountName = "cs2af83b2550815x4824x9e5";
            storageConfig.ImageContainer = "imagecontainer";
            storageConfig.AccountUri = new Uri("https://" + storageConfig.AccountName + ".blob.core.windows.net/");
            return storageConfig;
        }

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            try
            {
                // Create a URI to the blob
                Uri blobUri = new Uri("https://" +
                                      _storageConfig.AccountName +
                                      ".blob.core.windows.net/" +
                                      _storageConfig.ImageContainer +
                                      "/" + fileName);

                // Create StorageSharedKeyCredentials object by reading
                // the values from the configuration (appsettings.json)
                StorageSharedKeyCredential storageCredentials =
                    new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

                // Create the blob client.
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                // Upload the file
                await blobClient.UploadAsync(fileStream, true);
            }
            catch 
            {
                throw;
            }
            return await Task.FromResult(true);
        }

        public static void CopyDefaultImage(string customerCode, int inventoryId)
        {
            try
            {
                AzureStorageConfig storageConfig = AzureStorageConfigInstance();
                storageConfig.AccountKey = "Qh88kupOWr7yXUq2RPtBntfd1Mi2wJ+VAPxqzkr6PJRyiShnSQxNjuCDNQulSVAmozhuc/mTeZ3p+yxdjI/23Q==";
                storageConfig.AccountName = "cs2af83b2550815x4824x9e5";
                storageConfig.ImageContainer = "imagecontainer";

                StorageSharedKeyCredential storageCredentials =
                    new StorageSharedKeyCredential(storageConfig.AccountName, storageConfig.AccountKey);

                Uri needsImageUri = GetBlobUri(storageConfig, "needsImgB.jpg");
                Uri needsImageThumbnailUri = GetBlobUri(storageConfig, "needsImgB_t.jpg");
                Uri inventoryImageUri = GetBlobUri(storageConfig, customerCode + "_" + inventoryId);
                Uri inventoryImageThumbnailUri = GetBlobUri(storageConfig, customerCode + "_" + inventoryId + "_t");

                BlobClient inventoryImageBlob = new BlobClient(inventoryImageUri, storageCredentials);
                BlobClient inventoryImageThumbnailBlob = new BlobClient(inventoryImageThumbnailUri, storageCredentials);

                var x = inventoryImageBlob.StartCopyFromUri(needsImageUri);
                var y = inventoryImageThumbnailBlob.StartCopyFromUri(needsImageThumbnailUri);
            }
            catch
            {
                throw;
            }
        }

        private static Uri GetBlobUri(AzureStorageConfig storageConfig, string blobName)
        {
            return new Uri("https://" +
                          storageConfig.AccountName +
                          ".blob.core.windows.net/" +
                          storageConfig.ImageContainer +
                          "/" + blobName);
        }

        public static async void InventoryBlobCopy(int inventoryId, int destinationId)
        {
            AzureStorageConfig storageConfig = AzureStorageConfigInstance();
            List<string> names = GetBlobNames(storageConfig, inventoryId);
            foreach (string name in names)
            {
                string newName;
                if (name.EndsWith("_t"))
                {
                    newName = name.Substring(0, name.IndexOf('_') + 1) + destinationId + "_t";
                }
                else
                {
                    newName = name.Substring(0, name.IndexOf('_') + 1) + destinationId;
                }

                await CopyBlobIfExists(storageConfig, name, newName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageConfig"></param>
        /// <param name="inventoryId"></param>
        /// <returns>blob images for the specific inventory id, if they exist</returns>
        private static List<string> GetBlobNames(AzureStorageConfig storageConfig, int inventoryId)
        {
            List<string> blobNames = new List<string>();
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(storageConfig.AccountName, storageConfig.AccountKey);

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.AccountUri, storageCredentials);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(storageConfig.ImageContainer);

            // Find the specific large and thumbnail blobs (images) for this inventory id
            foreach (var blobItem in container.GetBlobs())
            {
                if (blobItem.Name.EndsWith("_" + inventoryId) || blobItem.Name.EndsWith("_" + inventoryId + "_t"))
                {
                    blobNames.Add(blobItem.Name);
                }
            }
            return blobNames;
        }

        private static async Task<string> CopyBlobIfExists(AzureStorageConfig storageConfig, string sourceBlobName, string destinationBlobName)
        {
            try
            {
                CloudStorageAccount storageAccount = new CloudStorageAccount(
                    new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey), true);
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer sourceContainer = cloudBlobClient.GetContainerReference(storageConfig.ImageContainer);
                CloudBlobContainer targetContainer = cloudBlobClient.GetContainerReference(storageConfig.ImageContainer);
                CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(sourceBlobName);
                CloudBlockBlob targetBlob = targetContainer.GetBlockBlobReference(destinationBlobName);
                await targetBlob.StartCopyAsync(sourceBlob);

                await targetBlob.FetchAttributesAsync();

                while (targetBlob.CopyState.Status == CopyStatus.Pending)
                {
                    Thread.Sleep(500);
                    await targetBlob.FetchAttributesAsync();
                }

                if (targetBlob.CopyState.Status != CopyStatus.Success)
                {
                    return targetBlob.CopyState.Status.ToString();
                }
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return "error " + s;
            }
            return "ok";
        }
    }
}
