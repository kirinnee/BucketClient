using System;
using System.Collections.Generic;

namespace BucketClient.AWS
{
    internal class AWSPolicyFactory
    {
        internal static Policy GeneratePolicy(ReadAccess access, string bucket)
        {
            List<PolicyStatement> statements = new List<PolicyStatement>();

            if (access == ReadAccess.Public)
            {
                statements.Add(new PolicyStatement(
                    "AllowPublicRead" + Guid.NewGuid().ToString(),
                    new string[] { "s3:GetObject" },
                    "Allow",
                    $"arn:aws:s3:::{bucket}/*",
                    "*"
                ));
            }
            return new Policy("ReadPolicy" + Guid.NewGuid().ToString(), "2012-10-17", statements.ToArray());
        }

        
    }
}
