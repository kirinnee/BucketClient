using Microsoft.WindowsAzure.Storage.Blob;
using MimeDetective;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient.Azure
{
    internal class AzureBucket : IBucket
    {
        private readonly CloudBlobClient _client;
        private readonly AzureBucketClient _bucketClient;
        private readonly CloudBlobContainer _bucket;

        public AzureBucket(CloudBlobContainer bucket, CloudBlobClient client, AzureBucketClient bucketClient)
        {
            _bucket = bucket;
            _client = client;
            _bucketClient = bucketClient;
        }

        public async Task<OperationResult> CreateBlob(byte[] payload, string key)
        {
            CloudBlockBlob blob = _bucket.GetBlockBlobReference(key);
            bool exist = await ExistBlob(key);
            if (exist) return new OperationResult(false, "Blob already exist", HttpStatusCode.BadRequest);
            return await PutBlob(payload, key);
        }

        public Task<OperationResult> CreateBlob(Stream payload, string key)
        {
            return CreateBlob(payload.ToByte(), key);
        }

        public async Task<OperationResult> DeleteBlob(string key)
        {
            try
            {
                CloudBlockBlob blob = _bucket.GetBlockBlobReference(key);
                bool pass = await blob.DeleteIfExistsAsync();
                if (pass) return new OperationResult(true, "", HttpStatusCode.OK);
                return new OperationResult(false, "Failed to delete blob", HttpStatusCode.BadRequest);
            }
            catch(Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
            
        }

        public Task<OperationResult> DeleteBlob(Uri key)
        {
            return _bucketClient.DeleteBlob(key);
        }

        public Task<bool> ExistBlob(string key)
        {
            try
            {
                CloudBlockBlob blob = _bucket.GetBlockBlobReference(key);
                return blob.ExistsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ExistBlob(Uri key)
        {
            return _bucketClient.ExistBlob(key);
        }

        public async Task<OperationResult> PutBlob(byte[] payload, string key)
        {
            try
            {
                CloudBlockBlob blob = _bucket.GetBlockBlobReference(key);
                await blob.UploadFromByteArrayAsync(payload, 0, payload.Length);
                blob.Properties.ContentType = payload.GetFileType().Mime;
                await blob.SetPropertiesAsync();
                return new OperationResult(true, "", HttpStatusCode.OK).AppendUri(blob.Uri);
            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, HttpStatusCode.BadRequest);
            }
        }

        public Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            return _bucketClient.PutBlob(payload, key);
        }

        public Task<OperationResult> PutBlob(Stream payload, string key)
        {
            return PutBlob(payload.ToByte(),key);
        }

        public Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            return PutBlob(payload.ToByte(), key);
        }

        public async Task<OperationResult> UpdateBlob(byte[] payload, string key)
        {
            bool exist = await ExistBlob(key);
            if (!exist) return new OperationResult(false, "Blob does not exist!", HttpStatusCode.NotFound);
            return await PutBlob(payload,key);
        }

        public Task<OperationResult> UpdateBlob(byte[] payload, Uri key)
        {
            return _bucketClient.UpdateBlob(payload, key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, string key)
        {
            return UpdateBlob(payload.ToByte(), key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            return UpdateBlob(payload.ToByte(), key);
        }
    }
}
