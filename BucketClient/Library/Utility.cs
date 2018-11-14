using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BucketClient
{
   
    internal static class Utility
    {
        internal static byte[] ToByte(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                input.Close();
                return ms.ToArray();
            }
        }

        internal static dynamic DeserializeXML(this string xmlData)
        {
            XDocument doc = XDocument.Parse(xmlData); //or XDocument.Load(path)
            string jsonText = JsonConvert.SerializeXNode(doc);
            dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            return dyn;
        }

        internal static string GenerateGetCORS(ReadAccess access)
        {
            if (access == ReadAccess.Public)
                return @"<CORSConfiguration>
                 <CORSRule>
                   <AllowedOrigin>*</AllowedOrigin>
                   <AllowedMethod>GET</AllowedMethod>
                   <AllowedHeader>*</AllowedHeader>
                   <MaxAgeSeconds>3000</MaxAgeSeconds>
                 </CORSRule>
                </CORSConfiguration>";
            else
                return @"<CORSConfiguration>
                </CORSConfiguration>";
        }

    }
}
