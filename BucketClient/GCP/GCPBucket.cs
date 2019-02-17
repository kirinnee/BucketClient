using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient.GCP
{
    internal class GCPBucket : IBucket
    {
        private readonly StorageClient _client;
        private readonly Bucket _bucket;
        private readonly IBucketClient _bucketClient;

        private Uri ToUri(string key)
        {
            return new Uri($"https://storage.googleapis.com/{_bucket.Name}/{key}");
        }

        public GCPBucket(StorageClient client, Bucket bucket, IBucketClient bucketClient)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            _bucketClient = bucketClient ?? throw new ArgumentNullException(nameof(bucketClient));
        }

        public Task<OperationResult> CreateBlob(byte[] payload, string key)
        {
            return CreateBlob(payload.ToStream(), key);
        }

        public async Task<OperationResult> CreateBlob(Stream payload, string key)
        {
            Uri endpoint = ToUri(key);
            bool exist = await _bucketClient.ExistBlob(endpoint);
            if (exist) return new OperationResult(false, "Blob already exist", HttpStatusCode.BadRequest);
            return await _bucketClient.PutBlob(payload, endpoint);
        }

        public Task<OperationResult> DeleteBlob(string key)
        {
            return DeleteBlob(ToUri(key));
        }

        public Task<OperationResult> DeleteBlob(Uri key)
        {
            return _bucketClient.DeleteBlob(key);
        }

        public Task<Uri> GetUri(string key)
        {
            return Task.FromResult(ToUri(key));
        }

        public Task<bool> ExistBlob(string key)
        {
            return ExistBlob(ToUri(key));
        }

        public Task<bool> ExistBlob(Uri key)
        {
            return _bucketClient.ExistBlob(key);
        }

        public Task<OperationResult> PutBlob(byte[] payload, string key)
        {
            return PutBlob(payload, ToUri(key));
        }

        public Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            return PutBlob(payload.ToStream(), key);
        }

        public Task<OperationResult> PutBlob(Stream payload, string key)
        {
            return PutBlob(payload, ToUri(key));
        }

        public Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            return _bucketClient.PutBlob(payload, key);
        }

        public Task<OperationResult> UpdateBlob(byte[] payload, string key)
        {
            return UpdateBlob(payload.ToStream(), key);
        }

        public Task<OperationResult> UpdateBlob(byte[] payload, Uri key)
        {
            return UpdateBlob(payload.ToStream(), key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, string key)
        {
            return UpdateBlob(payload, ToUri(key));
        }

        public Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            return _bucketClient.UpdateBlob(payload, key);
        }
    }
}