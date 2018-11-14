using BucketClient.AWS;
using BucketClient.DigitalOcean.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BucketClient.DigitalOcean
{

    internal class DigitalOceanBucketClient : IBucketClient
    {
        private readonly string _region;
        private readonly DigitalOceanHttpClient _client;



        public DigitalOceanBucketClient(HttpClient client, string accessKeyID, string accessKeySecret, string region)
        {

            var signer = new AWS4RequestSigner(accessKeyID, accessKeySecret);
            _client = new DigitalOceanHttpClient(client, signer, region);
            _region = region;
        }

        #region BUCKET

        public async Task<OperationResult> CreateBucket(string key)
        {
            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}";
            return await _client.SendRequest(HttpMethod.Put, endpoint);
        }
        public async Task<OperationResult> SetReadPolicy(string key, ReadAccess access)
        {

            Task<OperationResult> applyCORS = SetCORS(key, access, 10);
            Task<OperationResult> applyACL = SetACL(key, access, 10);
            OperationResult[] results = await Task.WhenAll(applyACL, applyCORS);

            OperationResult resp = await GetAllObjectURI(key);
            if(!resp.Success) return resp;

            IEnumerable<Uri> uris = resp.Message.Split('\n').Where(s => s.Trim() != "").Select(s => new Uri(s));
            foreach(Uri uri in uris)
            {
                OperationResult acl = await SetBlobACL(uri, access, 10);
                if (!acl.Success) return acl;
            }
            return new OperationResult(results.All(s => s.Success), string.Join("\n\n", results.Select(s => s.Message)), HttpStatusCode.BadRequest);

        }
        public async Task<OperationResult> DeleteBucket(string key)
        {
            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}";
            return await _client.SendRequest(HttpMethod.Delete, endpoint, null, HttpStatusCode.NoContent);
        }
        public async Task<bool> ExistBucket(string key)
        {
            var resp = await _client.SendRequest(HttpMethod.Head, $"https://{_region}.digitaloceanspaces.com/{key}");
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
            return Task.FromResult(new DigitalOceanBucket(key, _client, _region, this) as IBucket);
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
            
            var resp = await _client.SendRequest(HttpMethod.Put, key, payload,HttpStatusCode.OK);

            string bucket = key.AbsolutePath.Split('/').First();


            var highestLevelDomain = key.Host.Split('.').First();
            if (highestLevelDomain != _region) bucket = highestLevelDomain;
            bool isPub = await IsBucketPublic(bucket);

            ReadAccess access = isPub ? ReadAccess.Public : ReadAccess.Private;

            var acl = await SetBlobACL(key, access, 10);
            if (!acl.Success) return acl;
           

            return resp.AppendUri(key);
        }

        public Task<OperationResult> UpdateBlob(Stream payload, Uri key)
        {
            return UpdateBlob(payload.ToByte(), key);
        }

        public async Task<OperationResult> PutBlob(byte[] payload, Uri key)
        {
            var resp = await _client.SendRequest(HttpMethod.Put, key, payload);

            string bucket = key.AbsolutePath.Split('/').First();


            var highestLevelDomain = key.Host.Split('.').First();
            if (highestLevelDomain != _region) bucket = highestLevelDomain;
            bool isPub = await IsBucketPublic(bucket);

            ReadAccess access = isPub ? ReadAccess.Public : ReadAccess.Private;

            var acl = await SetBlobACL(key, access, 10);
            if (!acl.Success) return acl;

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
        internal async Task<bool> IsBucketPublic(string key)
        {
            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}/?acl=";

            var acl = await _client.SendRequest(HttpMethod.Get, endpoint);
            dynamic xml = acl.Message.DeserializeXML();
            dynamic dy = xml.AccessControlPolicy.AccessControlList.Grant;

            bool flag = false;
            try
            {
                foreach (var o in dy)
                {
                    try
                    {
                        string uri = o.Grantee.URI;
                        if (uri == "http://acs.amazonaws.com/groups/global/AllUsers")
                        {
                            flag = true;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch { }
            
            return flag;
        }

        private async Task<OperationResult> GetAllObjectURI(string key)
        {
            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}/";
            var resp = await _client.SendRequest(HttpMethod.Get, endpoint);
            if (!resp.Success) return resp;
            dynamic xml = resp.Message.DeserializeXML();
            try
            {
                dynamic contents = xml.ListBucketResult.Contents;
                List<string> uriList = new List<string>();
                foreach (var content in contents)
                {
                    try
                    {
                        string uri = endpoint + content.Key;
                        uriList.Add(uri);
                    }
                    catch
                    {

                    }
                }
                return new OperationResult(true, string.Join("\n", uriList.ToArray()), HttpStatusCode.OK);
            }
            catch
            {

            }

            return new OperationResult(true, "", HttpStatusCode.OK);

        }

        internal async Task<OperationResult> SetBlobACL(Uri key, ReadAccess access, int max, int count = 0)
        {
            if (++count == max) return new OperationResult(false, "Failed too many times due to conflict", HttpStatusCode.BadRequest);

            string endpoint = $"{key.ToString()}?acl=";

            OperationResult resp;
            var acl = await _client.SendRequest(HttpMethod.Get, endpoint);


            dynamic xml = acl.Message.DeserializeXML();

            // Finally, we get the Group value and display it.
            string ownerID = xml.AccessControlPolicy.Owner.ID;

            string aclData = DigitalOceanACLFactory.GenerateACL(access, ownerID);
            resp = await _client.SendRequest(HttpMethod.Put, endpoint, aclData);


            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                return await SetBlobACL(key, access, max, count);
            }
            return resp;

        }

        private async Task<OperationResult> SetACL(string key, ReadAccess access, int max, int count = 0)
        {
            if (++count == max) return new OperationResult(false, "Failed too many times due to conflict", HttpStatusCode.BadRequest);

            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}/?acl=";

            OperationResult resp;

            var acl = await _client.SendRequest(HttpMethod.Get, endpoint);


            dynamic xml = acl.Message.DeserializeXML();

            // Finally, we get the Group value and display it.
            string ownerID = xml.AccessControlPolicy.Owner.ID;
            Console.WriteLine(ownerID);

            string aclData = DigitalOceanACLFactory.GenerateACL(access, ownerID);
            resp = await _client.SendRequest(HttpMethod.Put, endpoint, aclData);


            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                return await SetACL(key, access, max, count);
            }
            return resp;

        }

        private async Task<OperationResult> SetCORS(string key, ReadAccess access, int max, int count = 0)
        {
            if (++count == max) return new OperationResult(false, "Failed too many times due to conflict", HttpStatusCode.BadRequest);

            string endpoint = $"https://{_region}.digitaloceanspaces.com/{key}/?cors=";

            OperationResult resp;
            if (access == ReadAccess.Public)
            {
                var corsContent = Utility.GenerateGetCORS(access);
                resp = await _client.SendRequest(HttpMethod.Put, endpoint, corsContent);

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

        #endregion
        
    }


}
