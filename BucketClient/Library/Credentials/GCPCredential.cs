using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.Library.Credentials
{
    public class GCPCredential : ICredential
    {
        internal string projectID;
        internal string secretJSON;

        public GCPCredential(string projectID, string secretJSON)
        {
            this.projectID = projectID ?? throw new ArgumentNullException(nameof(projectID));
            this.secretJSON = secretJSON ?? throw new ArgumentNullException(nameof(secretJSON));
        }
    }
}
