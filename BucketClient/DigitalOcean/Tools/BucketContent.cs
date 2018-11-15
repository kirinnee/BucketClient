using BucketClient.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.DigitalOcean.Tools
{
    public class BucketContent
    {
        public ListBucketResult ListBucketResult; 
    }

    public class ListBucketResult
    {
        public string Name;
        public string Prefix;
        public string Marker;
        public string MaxKeys;
        public bool IsTruncated;

        [JsonConverter(typeof(SingleValueArrayConverter<Contents>))]
        public IEnumerable<Contents> Contents = new LinkedList<Contents>();
    }

    public class Contents
    {
        public string Key;
        public string LastModified;
        public string ETag;
        public int Size;
        public string StorageClass;
        public Owner owner;
    }
}
