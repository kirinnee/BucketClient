using System;
using System.Collections.Generic;
using System.Text;

namespace BucketClient.DigitalOcean.Tools
{
    internal class DigitalOceanACLFactory
    {
        internal static string GenerateACL(ReadAccess access, string ownerId)
        {
            if(access == ReadAccess.Public)
            {
                return $@"<AccessControlPolicy xmlns=""http://s3.amazonaws.com/doc/2006-03-01/"">
  <Owner>
    <ID>{ownerId}</ID>
  </Owner>
  <AccessControlList>
    <Grant>
      <Grantee xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""CanonicalUser"">
        <ID>{ownerId}</ID>
      </Grantee>
      <Permission>FULL_CONTROL</Permission>
    </Grant>
    <Grant>
      <Grantee xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""Group"">
        <URI>http://acs.amazonaws.com/groups/global/AllUsers</URI>
      </Grantee>
      <Permission>READ</Permission>
    </Grant>
  </AccessControlList>
</AccessControlPolicy>";
            }
            else
            {
                return $@"<AccessControlPolicy xmlns=""http://s3.amazonaws.com/doc/2006-03-01/"">
  <Owner>
    <ID>{ownerId}</ID>
  </Owner>
  <AccessControlList>
    <Grant>
      <Grantee xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""CanonicalUser"">
        <ID>{ownerId}</ID>
      </Grantee>
      <Permission>FULL_CONTROL</Permission>
    </Grant>
  </AccessControlList>
</AccessControlPolicy>";
            }
        }

    }
}
