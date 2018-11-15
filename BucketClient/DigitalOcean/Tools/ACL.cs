using BucketClient.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BucketClient.DigitalOcean.Tools
{
    public class ACL
    {
        public AccessControlPolicy AccessControlPolicy;
    }

    public class AccessControlPolicy
    {
        public AccessControlList AccessControlList;
        public Owner Owner;
    }


    public class AccessControlList
    {
        [JsonConverter(typeof(SingleValueArrayConverter<Grant>))]
        public IEnumerable<Grant> Grant;
    }


    public class Owner
    {
        public int ID;
        public string DiplayName;
    }


    public class Grant
    {
        public Grantee Grantee;
        public string Permission;
    }

    public class Grantee
    {
        public string URI;
        public string ID;
        public string DisplayName;
    }
}
