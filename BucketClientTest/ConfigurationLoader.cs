using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BucketClientTest
{
    public static class ConfigurationLoader
    {
        public static dynamic LoadConfiguration(string path)
        {
            string content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../../../", path));
            return JsonConvert.DeserializeObject<dynamic>(content);
        }

        public static byte[] LoadBlobAsBytes(string path)
        {
            return File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "../../../", path));
            
        }
    }
}
