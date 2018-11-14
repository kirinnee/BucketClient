using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient.AWS
{
   
    internal class AWSBucketClient : IBucketClient
    {
        private readonly string _region;
        private readonly AWSHttpClient _client;



        public AWSBucketClient(HttpClient client, string accessKeyID, string accessKeySecret, string region)
        {

            var signer = new AWS4RequestSigner(accessKeyID, accessKeySecret);
            _client = new AWSHttpClient(client, signer, region);
            _region = region;
        }

        #region BUCKET

        public async Task<OperationResult> CreateBucket(string key)
        {

            string content = $@"<CreateBucketConfiguration xmlns='http://s3.amazonaws.com/doc/2006-03-01/'>
                                    <LocationConstraint>{_region}</LocationConstraint>
                                </CreateBucketConfiguration>";
            string endpoint = $"https://s3-{_region}.amazonaws.com/{key}";
            return await _client.SendRequest(HttpMethod.Put, endpoint, content);
        }
        public async Task<OperationResult> SetReadPolicy(string key, ReadAccess access)
        {
            Task<OperationResult> applyCORS = SetCORS(key, access, 10);
            Task<OperationResult> applyPolicy = SetPolicy(key, access, 10);

            OperationResult[] results = await Task.WhenAll(applyPolicy, applyCORS);
            return new OperationResult(results.All(s => s.Success), string.Join("\n\n", results.Select(s => s.Message)), HttpStatusCode.BadRequest);
        }
        public async Task<OperationResult> DeleteBucket(string key)
        {
            string endpoint = $"https://s3-{_region}.amazonaws.com/{key}";
            return await _client.SendRequest(HttpMethod.Delete, endpoint, null, HttpStatusCode.NoContent);
        }
        public async Task<bool> ExistBucket(string key)
        {
            var resp = await _client.SendRequest(HttpMethod.Head, $"https://s3-{_region}.amazonaws.com/{key}");
            return resp.Success;
        }
        public async Task<IBucket> GetBucket(string key)
        {
            bool exist = await ExistBucket(key);
            if (!exist) return null;
            return await UnsafeGetBucket(key);
        }
        public Task<IBucket> UnsafeGetBucket(string key)
        {
            return Task.FromResult(new AWSBucket(key, _client, _region, this) as IBucket);
        }

        #endregion

        #region BLOB


        public async Task<OperationResult> DeleteBlob(Uri key)
        {
            return await _client.SendRequest(HttpMethod.Delete, key, null, HttpStatusCode.NoContent);
        }

        public async Task<OperationResult> UpdateBlob(byte[] payload, Uri key)
        {
            bool exist = await ExistBlob(key);
            if (!exist) return new OperationResult(false, $"Blob {key} does not exist.", HttpStatusCode.NotFound);
            var resp=  await _client.SendRequest(HttpMethod.Put, key, payload);
            return resp.AppendUri(key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            return UpdateBlob(payload.ToByte(), key);
        }
        
        public async Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            var resp =  await _client.SendRequest(HttpMethod.Put, key, payload);
            return resp.AppendUri(key);
        }

        public Task<OperationResult> PutBlob(Stream payload, Uri key)
        {
            return PutBlob(payload.ToByte(), key);
        }

        public async Task<bool> ExistBlob(Uri key)
        {

            var resp = await _client.SendRequest(HttpMethod.Head, key);
            return resp.Success;
        }

        #endregion

        #region ENCAPSULATED

        private async Task<OperationResult> SetPolicy(string key, ReadAccess access, int max, int count = 0)
        {
            if (++count == max) return new OperationResult(false, "Failed too many times due to conflict", HttpStatusCode.BadRequest);

            OperationResult resp;
            string endpoint = $"https://s3-{_region}.amazonaws.com/{key}/?policy=";
            if (access == ReadAccess.Public)
            {
                var policy = AWSPolicyFactory.GeneratePolicy(access, key);
                var content = JsonConvert.SerializeObject(policy);
                resp = await _client.SendRequest(HttpMethod.Put, endpoint, content, "application/json", HttpStatusCode.NoContent);
            }
            else
            {
                resp = await _client.SendRequest(HttpMethod.Delete, endpoint, null, "text/plain", HttpStatusCode.NoContent);
            }
            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                return await SetCORS(key, access, max, count);
            }
            return resp;

        }

        private async Task<OperationResult> SetCORS(string key, ReadAccess access, int max, int count = 0)
        {
            if (++count == max) return new OperationResult(false, "Failed too many times due to conflict", HttpStatusCode.BadRequest);

            string endpoint = $"https://s3-{_region}.amazonaws.com/{key}/?cors=";

            OperationResult resp;
            if (access == ReadAccess.Public)
            {
                var corsContent = Utility.GenerateGetCORS(access);
                resp =  await _client.SendRequest(HttpMethod.Put, endpoint, corsContent);
                
            }
            else
            {
                resp = await _client.SendRequest(HttpMethod.Delete, endpoint, null, "text/plain", HttpStatusCode.NoContent);
            }
            if(resp.StatusCode== HttpStatusCode.Conflict)
            {
                return await SetCORS(key, access, max, count);
            }
            return resp;

        }

        #endregion

      

      
    }


}
