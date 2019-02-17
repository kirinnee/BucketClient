using BucketClient.AWS;
using MimeDetective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient.DigitalOcean
{
    internal class DigitalOceanHttpClient
    {
        private readonly HttpClient _client;
        private readonly AWS4RequestSigner _signer;
        private readonly string _region;

        public DigitalOceanHttpClient(HttpClient client, AWS4RequestSigner signer, string region)
        {
            _client = client;
            _signer = signer;
            _region = region;
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, string endpoint, byte[] content,
            HttpStatusCode successDef = HttpStatusCode.OK, IDictionary<string, string> additionalHeaders = null)
        {
            return await SendRequest(method, new Uri(endpoint), content, successDef);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, byte[] content,
            HttpStatusCode successDef = HttpStatusCode.OK, IDictionary<string, string> additionalHeaders = null)
        {
            var c = content != null ? new ByteArrayContent(content) : null;
            string mime = content != null ? content.GetFileType().Mime : "application/xml";
            return await SendRequest(method, endpoint, c, mime, successDef);
        }

        public async Task<byte[]> GetObjectBinary(Uri key)
        {
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = key,
            };

            message = await _signer.Sign(message, "s3", _region);
            var response = await _client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, HttpContent content,
            string type = "application/xml", HttpStatusCode successDef = HttpStatusCode.OK,
            IDictionary<string, string> additionalHeaders = null)
        {
            var message = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = endpoint,
            };
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(type));
            if (additionalHeaders != null)
                additionalHeaders.Select(kv => message.Headers.TryAddWithoutValidation(kv.Key, kv.Value));
            if (content != null) message.Content = content;
            if (content != null) message.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(type);
            message = await _signer.Sign(message, "s3", _region);
            var response = await _client.SendAsync(message);
            string resp = await response.Content.ReadAsStringAsync();
            var success = response.StatusCode == successDef;
            return new OperationResult(success, resp, response.StatusCode);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, string endpoint, string content = null,
            string type = "text/plain", HttpStatusCode successDef = HttpStatusCode.OK,
            IDictionary<string, string> additionalHeaders = null)
        {
            return await SendRequest(method, new Uri(endpoint), content, type, successDef);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, string content = null,
            string type = "text/plain", HttpStatusCode successDef = HttpStatusCode.OK,
            IDictionary<string, string> additionalHeaders = null)
        {
            var c = content == null ? null : new StringContent(content, Encoding.UTF8, type);
            return await SendRequest(method, endpoint, c, type, successDef);
        }
    }
}