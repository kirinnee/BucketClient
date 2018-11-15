using BucketClient;
using BucketClient.Library.Credentials;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp
{
    internal static class Util
    {
        internal static CloudServiceProvider ToEnum(this string provider)
        {
            provider = provider.ToLower().Replace("-", "");
            switch (provider)
            {
                case "aws": goto case "amazon";
                case "amazon":
                    return CloudServiceProvider.AWS;
                case "digital": goto case "do";
                case "ocean": goto case "do";
                case "digitalocean": goto case "do";
                case "do":
                    return CloudServiceProvider.DigitalOcean;
                case "azure": goto case "ms";
                case "microsoft": goto case "ms";
                case "ms":
                    return CloudServiceProvider.Azure;
                case "google": goto case "gcp";
                case "gcp":
                    return CloudServiceProvider.GCP;
                default: throw new InvalidOperationException("No such provider:" + provider);
            }
        }
    }
    internal class Factory
    {
        private IBucketClient AWSBucket;
        private IBucketClient DOBucket;
        private IBucketClient AzureBucket;
        private IBucketClient GCPBucket;
        public Factory(ICredential awsCred, ICredential doCred, ICredential azureCred, ICredential gcpCred)
        {
            AWSBucket = BucketClientFactory.CreateClient(CloudServiceProvider.AWS, awsCred);
            DOBucket = BucketClientFactory.CreateClient(CloudServiceProvider.DigitalOcean, doCred);
            AzureBucket = BucketClientFactory.CreateClient(CloudServiceProvider.Azure, azureCred);
            GCPBucket = BucketClientFactory.CreateClient(CloudServiceProvider.GCP, gcpCred);
        }




        public IBucketClient GetBucket(CloudServiceProvider provider)
        {
            switch (provider)
            {
                case CloudServiceProvider.AWS:
                    return AWSBucket;
                case CloudServiceProvider.Azure:
                    return AzureBucket;
                case CloudServiceProvider.GCP:
                    return GCPBucket;
                case CloudServiceProvider.DigitalOcean:
                    return DOBucket;
                case CloudServiceProvider.AliCloud:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("No such provider: " + provider.ToString());
            }
        }
    }

    internal class Program
    {


        private static void Main(string[] args)
        {
            dynamic config = LoadConfiguration("secrets.json");
            dynamic aws = config.aws;
            dynamic DO = config.DO;
            dynamic azure = config.azure;
            dynamic gcp = config.gcp;

            ICredential awsCred = new AWSCredential(aws.id.ToString(), aws.key.ToString(), aws.region.ToString());
            ICredential doCred = new DigitalOceanCredential(DO.id.ToString(), DO.key.ToString(), DO.region.ToString());
            ICredential azureCred = new AzureCredential(azure.id.ToString(), azure.key.ToString());
            ICredential gcpCred = new GCPCredential(gcp.id.ToString(), JsonConvert.SerializeObject(gcp.secret));

            Factory fac = new Factory(awsCred, doCred,azureCred,gcpCred);

            AsyncMain(args, fac).GetAwaiter().GetResult();
        }

        public static dynamic LoadConfiguration(string path)
        {
            string content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../../../", path));
            return JsonConvert.DeserializeObject<dynamic>(content);
        }



        private static async Task AsyncMain(string[] arg, Factory fac)
        {


            var argument = Console.ReadLine();
            var argo = argument.Split(' ').Where(x => x.Trim() != "").ToArray();
            var clientType = argo[0];
            var args = argo.Skip(1).ToArray();
            try
            {
                IBucketClient client = fac.GetBucket(clientType.ToEnum());
                switch (args[0].ToLower())
                {
                    case "bucket":
                        switch (args[1].ToLower())
                        {
                            case "exist":
                                var exist = await client.ExistBucket(args[2]);
                                Console.WriteLine(args[2] + " " + (exist ? "exists!" : "does not exist"));
                                break;
                            case "create":
                                var createResp = await client.CreateBucket(args[2]);
                                Console.WriteLine(JsonConvert.SerializeObject(createResp));
                                break;
                            case "del": goto case "delete";
                            case "delete":
                                var delResp = await client.DeleteBucket(args[2]);
                                Console.WriteLine(JsonConvert.SerializeObject(delResp));
                                break;
                            case "policy":
                                var policyResp = await client.SetReadPolicy(args[2], args[3] == "public" ? ReadAccess.Public : ReadAccess.Private);
                                Console.WriteLine(JsonConvert.SerializeObject(policyResp));
                                break;
                            default:
                                Console.WriteLine("Unknown command!");
                                break;
                        }
                        break;
                    case "blob":
                        var bucketKey = args[2];
                        var bucket = await client.GetBucket(bucketKey);
                        var argv = args[3];
                        switch (args[1].ToLower())
                        {
                            case "exist":
                                var exist = await bucket.ExistBlob(argv);
                                Console.WriteLine(argv + " " + (exist ? "exists!" : "does not exist"));
                                break;
                            case "create":
                                byte[] file = File.ReadAllBytes($@"{Directory.GetCurrentDirectory()}/{args[4]}");
                                var createResp = await bucket.CreateBlob(file, argv);
                                Console.WriteLine(JsonConvert.SerializeObject(createResp));
                                break;
                            case "del": goto case "delete";
                            case "delete":
                                var delResp = await bucket.DeleteBlob(argv);
                                Console.WriteLine(JsonConvert.SerializeObject(delResp));
                                break;
                            case "update":
                                byte[] updateFile = File.ReadAllBytes($@"{Directory.GetCurrentDirectory()}/{args[4]}");
                                var updateResp = await bucket.UpdateBlob(updateFile, argv);
                                Console.WriteLine(JsonConvert.SerializeObject(updateResp));
                                break;
                            case "put":
                                byte[] putFile = File.ReadAllBytes($@"{Directory.GetCurrentDirectory()}/{args[4]}");
                                var putResp = await bucket.PutBlob(putFile, argv);
                                Console.WriteLine(JsonConvert.SerializeObject(putResp));
                                break;
                            default:
                                Console.WriteLine("Unknown command!");
                                break;
                        }
                        break;
                    case "test":
                        byte[] putFileTest = File.ReadAllBytes($@"{Directory.GetCurrentDirectory()}/{args[3]}");
                        var putRespTest = await client.PutBlob(putFileTest, new Uri($"https://{LoadConfiguration("secrets.json").DO.region.ToString()}.digitaloceanspaces.com/{args[1]}/{args[2]}"));
                        Console.WriteLine(JsonConvert.SerializeObject(putRespTest));
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Invalid number of arguments");

            }
            await AsyncMain(arg, fac);
        }
    }
}
