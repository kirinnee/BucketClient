using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.Library.Credentials
{
    public class AzureCredential : ICredential
    {
        
        internal string AccountName { get; private set; }
        internal string Secret { get; private set; }

        public AzureCredential(string accountName, string secret)
        {
            AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
            Secret = secret ?? throw new ArgumentNullException(nameof(secret));
        }
    }
}
