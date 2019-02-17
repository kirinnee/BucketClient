using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BucketClient.AWS
{
    public class AWSBucket : IBucket
    {
        private readonly string _region;
        private readonly AWSHttpClient _client;
        private readonly string _key;
        private readonly IBucketClient _bucketClient;

        internal AWSBucket(string key, AWSHttpClient client, string region, AWSBucketClient bucketClient)
        {
            _region = region;
            _client = client;
            _key = key;
            _bucketClient = bucketClient;
        }

        public async Task<OperationResult> CreateBlob(byte[] payload, string key)
        {
            bool exist = await ExistBlob(key);
            if (exist) return new OperationResult(false, "Object already exist", HttpStatusCode.BadRequest);
            string endpoint = $"https://s3-{_region}.amazonaws.com/{_key}/{key}";
            var resp = await _client.SendRequest(HttpMethod.Put, endpoint, payload);
            return resp.Success ? resp.AppendUri(endpoint) : resp;
        }

        public Task<OperationResult> CreateBlob(Stream payload, string key)
        {
            return CreateBlob(payload.ToByte(), key);
        }

        public Task<OperationResult> DeleteBlob(string key)
        {
            return DeleteBlob(new Uri($"https://s3-{_region}.amazonaws.com/{_key}/{key}"));
        }

        public Task<OperationResult> DeleteBlob(Uri key)
        {
            return _bucketClient.DeleteBlob(key);
        }

        public Task<Uri> GetUri(string key)
        {
            return Task.FromResult(new Uri($"https://s3-{_region}.amazonaws.com/{_key}/{key}"));
        }

        public Task<bool> ExistBlob(string key)
        {
            return ExistBlob(new Uri($"https://s3-{_region}.amazonaws.com/{_key}/{key}"));
        }

        public Task<bool> ExistBlob(Uri key)
        {
            return _bucketClient.ExistBlob(key);
        }

        public Task<OperationResult> PutBlob(byte[] payload, string key)
        {
            return PutBlob(payload, new Uri($"https://s3-{_region}.amazonaws.com/{_key}/{key}"));
        }

        public Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            return _bucketClient.PutBlob(payload, key);
        }

        public Task<OperationResult> PutBlob(Stream payload, string key)
        {
            return PutBlob(payload.ToByte(), key);
        }

        public Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            return PutBlob(payload.ToByte(), key);
        }

        public Task<OperationResult> UpdateBlob(byte[] payload, string key)
        {
            return UpdateBlob(payload, new Uri($"https://s3-{_region}.amazonaws.com/{_key}/{key}"));
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
