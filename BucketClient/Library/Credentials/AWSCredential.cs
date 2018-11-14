using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.Library.Credentials
{
    public class AWSCredential : ICredential
    {
        internal string accessKeyID;
        internal string accessKeySecret;
        internal string region;

        public AWSCredential(string accessKeyID, string accessKeySecret, string region)
        {
            this.accessKeyID = accessKeyID ?? throw new ArgumentNullException(nameof(accessKeyID));
            this.accessKeySecret = accessKeySecret ?? throw new ArgumentNullException(nameof(accessKeySecret));
            this.region = region ?? throw new ArgumentNullException(nameof(region));
        }
    }
}
