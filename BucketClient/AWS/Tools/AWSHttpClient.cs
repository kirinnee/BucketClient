﻿using MimeDetective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient.AWS
{
    internal class AWSHttpClient
    {
        private readonly HttpClient _client;
        private readonly AWS4RequestSigner _signer;
        private readonly string _region;

        public AWSHttpClient(HttpClient client, AWS4RequestSigner signer, string region)
        {
            _client = client;
            _signer = signer;
            _region = region;
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, string endpoint, byte[] content,
            HttpStatusCode successDef = HttpStatusCode.OK)
        {
            return await SendRequest(method, new Uri(endpoint), content, successDef);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, byte[] content,
            HttpStatusCode successDef = HttpStatusCode.OK)
        {
            var c = content != null ? new ByteArrayContent(content) : null;
            var mime = content != null ? content.GetFileType().Mime : "text/plain";
            return await SendRequest(method, endpoint, c, mime, successDef);
        }

        public async Task<HttpResponseMessage> Ping(HttpMethod method, Uri endpoint, HttpContent content,
            string type = "text/plain")
        {
            var message = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = endpoint,
            };
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(type));
            if (content != null) message.Content = content;
            if (type != "text/plain") message.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(type);
            message = await _signer.Sign(message, "s3", _region);
            return await _client.SendAsync(message);
        }

        private async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, HttpContent content,
            string type = "text/plain", HttpStatusCode successDef = HttpStatusCode.OK)
        {
            var response = await Ping(method, endpoint, content, type);
            var resp = await response.Content.ReadAsStringAsync();
            var success = response.StatusCode == successDef;
            return new OperationResult(success, resp, response.StatusCode);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, string endpoint, string content = null,
            string type = "text/plain", HttpStatusCode successDef = HttpStatusCode.OK)
        {
            return await SendRequest(method, new Uri(endpoint), content, type, successDef);
        }

        public async Task<OperationResult> SendRequest(HttpMethod method, Uri endpoint, string content = null,
            string type = "text/plain", HttpStatusCode successDef = HttpStatusCode.OK)
        {
            var c = content == null ? null : new StringContent(content, Encoding.UTF8, type);
            return await SendRequest(method, endpoint, c, type, successDef);
        }
    }
}