using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using MimeDetective;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Google.Apis.Storage.v1.Data.Bucket;

namespace BucketClient.GCP
{
    internal class GCPBucketClient : IBucketClient
    {
        private readonly StorageClient _client;
        private readonly string _projectID;

        public GCPBucketClient(string projectID, string secretJson)
        {
            _projectID = projectID;
            _client = StorageClient.Create(GoogleCredential.FromJson(secretJson));
        }


        public async Task<byte[]> GetBlob(Uri key)
        {
            var bucket = GetBucketName(key);
            var blob = GetObjectName(key);
            using (var stream = new MemoryStream())
            {
                await _client.DownloadObjectAsync(bucket, blob, stream);
                return stream.ToArray();
            }
        }

        #region BUCKET

        public async Task<OperationResult> CreateBucket(string key)
        {
            try
            {
                Bucket bucket = await _client.CreateBucketAsync(_projectID, key);

                return new OperationResult(true, "Created bucket", HttpStatusCode.OK);
            }
            catch (GoogleApiException e)
            {
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }

        public async Task<OperationResult> DeleteBucket(string key)
        {
            try
            {
                await _client.DeleteBucketAsync(key);
                return new OperationResult(true, "Deleted bucket", HttpStatusCode.OK);
            }
            catch (GoogleApiException e)
            {
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }


        public async Task<bool> ExistBucket(string key)
        {
            try
            {
                Bucket bucket = await _client.GetBucketAsync(key);
                return bucket != null;
            }
            catch (GoogleApiException e)
                when (e.Error.Code == 404)
            {
                return false;
            }
        }

        public async Task<IBucket> GetBucket(string key)
        {
            Bucket bucket = await _client.GetBucketAsync(key);
            return new GCPBucket(_client, bucket, this);
        }

        public async Task<OperationResult> SetReadPolicy(string key, ReadAccess access)
        {
            try
            {
                Bucket bucket = await _client.GetBucketAsync(key);
                PredefinedBucketAcl bacl = access == ReadAccess.Public
                    ? PredefinedBucketAcl.PublicRead
                    : PredefinedBucketAcl.Private;
                PredefinedObjectAcl oacl = access == ReadAccess.Public
                    ? PredefinedObjectAcl.PublicRead
                    : PredefinedObjectAcl.Private;
                _client.UpdateBucket(bucket, new UpdateBucketOptions()
                {
                    PredefinedAcl = bacl,
                    PredefinedDefaultObjectAcl = oacl
                });
                var buckets = _client.ListObjects(key).ReadPage(Int32.MaxValue / 2);
                IEnumerable<Task> tasks = buckets.Select(s =>
                    _client.UpdateObjectAsync(s, new UpdateObjectOptions() {PredefinedAcl = oacl}));
                await Task.WhenAll(tasks);
                return new OperationResult(true, "", HttpStatusCode.OK);
            }
            catch (GoogleApiException e)
            {
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }

        public async Task<IBucket> UnsafeGetBucket(string key)
        {
            Bucket bucket = await _client.GetBucketAsync(key);
            return new GCPBucket(_client, bucket, this);
        }


        public async Task<OperationResult> SetGETCors(string key, string[] cors)
        {
            try
            {
                Bucket bucket = await _client.GetBucketAsync(key);
                if (bucket.Cors == null) bucket.Cors = new List<CorsData>();
                bucket.Cors.Clear();
                bucket.Cors.Add(new CorsData()
                {
                    Method = new List<string>() {"GET"},
                    Origin = cors.ToList(),
                    ResponseHeader = new List<string>() {"*"}
                });
                await _client.UpdateBucketAsync(bucket);
                return new OperationResult(true, "", HttpStatusCode.OK);
            }
            catch (GoogleApiException e)
            {
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }

        #endregion

        #region BLOB

        public async Task<OperationResult> DeleteBlob(Uri key)
        {
            try
            {
                string bucket = GetBucketName(key);
                string blob = GetObjectName(key);
                await _client.DeleteObjectAsync(bucket, blob);
                return new OperationResult(true, "", HttpStatusCode.OK);
            }
            catch (GoogleApiException e)
            {
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }


        public async Task<bool> ExistBlob(Uri key)
        {
            string bucket = GetBucketName(key);
            string blob = GetObjectName(key);
            try
            {
                var obj = await _client.GetObjectAsync(bucket, blob);
                return bucket != null;
            }
            catch (GoogleApiException e)
                when (e.Error.Code == 404)
            {
                return false;
            }
        }

        public Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            return PutBlob(payload.ToStream(), key);
        }

        public async Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            try
            {
                var bucket = GetBucketName(key);
                var blob = GetObjectName(key);
                var exist = await ExistBlob(key);
                Uri link;
                if (!exist)
                {
                    var bExist = await ExistBucket(bucket);
                    if (!bExist)
                    {
                        await _client.CreateBucketAsync(_projectID, bucket);
                    }
                }

                var obj = await _client.UploadObjectAsync(bucket, blob, payload.ToByteUnsafe().GetFileType().Mime,
                    payload);
                obj.CacheControl = "no-cache";
                await _client.UpdateObjectAsync(obj);
                link = new Uri($"https://storage.googleapis.com/{bucket}/{blob}");
                payload.Close();
                return new OperationResult(true, "Blob created", HttpStatusCode.OK).AppendUri(link);
            }
            catch (GoogleApiException e)
            {
                payload.Close();
                return new OperationResult(false, e.Message, (HttpStatusCode) e.Error.Code);
            }
        }

        public Task<OperationResult> UpdateBlob(byte[] payload, Uri key)
        {
            return UpdateBlob(payload.ToStream(), key);
        }

        public async Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            bool exist = await ExistBlob(key);
            if (!exist) return new OperationResult(false, "Blob does not exist", HttpStatusCode.NotFound);
            return await PutBlob(payload, key);
        }

        #endregion

        #region ENCAPSULATED

        public string GetBucketName(Uri fullUri)
        {
            return fullUri.AbsolutePath.Split('/').Where(s => s.Trim() != "").First();
        }

        public string GetObjectName(Uri fullUri)
        {
            return string.Join("/", fullUri.AbsolutePath.Split('/').Where(s => s.Trim() != "").Skip(1));
        }

        #endregion
    }
}