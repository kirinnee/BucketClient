using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.DigitalOcean.Tools
{
    public class ACL
    {
        public AccessControlPolicy AccessControlPolicy;
    }

    public class AccessControlPolicy
    {
        public IEnumerable<Grant> AccessControlList;
        public Owner Owner;
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
