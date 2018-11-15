using Newtonsoft.Json;
using System.Dynamic;
using System.IO;
using System.Xml;
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

        internal static byte[] ToByteUnsafe(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        internal static Stream ToStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        internal static T DeserializeXMLString<T>(this string xmlData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlData);
            string json = JsonConvert.SerializeXmlNode(doc);
            T t =  JsonConvert.DeserializeObject<T>(json);
            return t;
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
