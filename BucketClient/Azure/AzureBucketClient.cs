using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeDetective;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BucketClient.Azure
{
    internal class AzureBucketClient : IBucketClient
    {
        private CloudBlobClient _client;

        public AzureBucketClient(string accountName, string secret)
        {
            var credential = new StorageCredentials(accountName, secret);
            var storage = new CloudStorageAccount(credential, true);
            _client = storage.CreateCloudBlobClient();

        }

        #region BUCKET

        public async Task<OperationResult> CreateBucket(string key)
        {
            try
            {
                CloudBlobContainer container = _client.GetContainerReference(key);
                bool success = await container.CreateIfNotExistsAsync();
                if (!success) return new OperationResult(false, "Failed to create bucket", HttpStatusCode.BadRequest);
                return new OperationResult(true, "", HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
        }



        public async Task<OperationResult> DeleteBucket(string key)
        {
            try
            {
                CloudBlobContainer container = _client.GetContainerReference(key);
                bool success = await container.DeleteIfExistsAsync();
                if (!success) return new OperationResult(false, "Failed to delete bucket", HttpStatusCode.BadRequest);
                return new OperationResult(true, "", HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
        }

        public Task<bool> ExistBucket(string key)
        {
            CloudBlobContainer container = _client.GetContainerReference(key);
            return container.ExistsAsync();
        }

        public async Task<IBucket> GetBucket(string key)
        {
            bool exist = await ExistBucket(key);
            if (!exist) return null;
            CloudBlobContainer bucket = _client.GetContainerReference(key);
            return new AzureBucket(bucket, _client, this);

        }

        public async Task<OperationResult> SetReadPolicy(string key, ReadAccess access)
        {
            try
            {
                bool exist = await ExistBucket(key);
                if (!exist) return new OperationResult(false, "Bucket does not exist", HttpStatusCode.NotFound);
                CloudBlobContainer bucket = _client.GetContainerReference(key);
                BlobContainerPermissions perm = await bucket.GetPermissionsAsync();
                perm.PublicAccess = access == ReadAccess.Public ? BlobContainerPublicAccessType.Blob : BlobContainerPublicAccessType.Off;
                await bucket.SetPermissionsAsync(perm);
                return new OperationResult(true, "Permission is now " + access.ToString(), HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
        }

        public Task<IBucket> UnsafeGetBucket(string key)
        {
            CloudBlobContainer bucket = _client.GetContainerReference(key);
            return Task.FromResult(new AzureBucket(bucket, _client, this) as IBucket);
        }

        #endregion

        #region BLOB

        public async Task<OperationResult> DeleteBlob(Uri key)
        {
            try
            {
                ICloudBlob blob = await _client.GetBlobReferenceFromServerAsync(key);
                bool success = await blob.DeleteIfExistsAsync();
                if (success) return new OperationResult(true, "", HttpStatusCode.OK);
                return new OperationResult(false, "", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }

        }

        public async Task<bool> ExistBlob(Uri key)
        {
            ICloudBlob blob = await _client.GetBlobReferenceFromServerAsync(key);
            return await blob.ExistsAsync();
        }

        public async Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            try
            {
                ICloudBlob blob = await _client.GetBlobReferenceFromServerAsync(key);
                await blob.UploadFromByteArrayAsync(payload, 0, payload.Length);
                blob.Properties.ContentType = payload.GetFileType().Mime;
                await blob.SetPropertiesAsync();
                return new OperationResult(true, "", HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
            
        }

        public Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            return PutBlob(payload.ToByte(), key);
        }



        public async Task<OperationResult> UpdateBlob(byte[] payload, Uri key)
        {
            bool exist = await ExistBlob(key);
            if (!exist) return new OperationResult(false, "Blob does not exist", HttpStatusCode.NotFound);
            return await PutBlob(payload, key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            return UpdateBlob(payload.ToByte(), key);
        }

        #endregion
    }
}
