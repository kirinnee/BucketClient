using System;
using System.Net;

namespace BucketClient
{
    public class OperationResult
    {


        public bool Success { get; private set; }
        public string Message { get; private set; }

        public Uri Usable { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public OperationResult(bool success, string message, HttpStatusCode code, string uri = null)
        {
            Success = success;
            Message = message;
            StatusCode = code;
            if (uri != null) Usable = new Uri(uri);
        }

        public OperationResult(bool success, string message, HttpStatusCode code, Uri uri)
        {
            Success = success;
            Message = message;
            StatusCode = code;
            if (uri != null) Usable = uri;
        }

        internal OperationResult AppendUri(string uri)
        {
            return AppendUri( new Uri(uri));
        }

        internal OperationResult AppendUri(Uri uri)
        {
            Usable = uri;
            return this;
        }


    }
}
