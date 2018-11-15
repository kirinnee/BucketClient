using BucketClient;
using BucketClient.Library.Credentials;
using System;
using Xunit;
using Xunit.Abstractions;

namespace BucketClientTest
{
    public class IntegrationTest
    {
        private readonly IBucketClient awsClient;
        private readonly IBucketClient doClient;
        private readonly IBucketClient azureClient;
        private readonly string awsTargetBucket;
        private readonly string doTargetBucket;
        private readonly string azureTargetBucket;
        private ITestOutputHelper _output;

        public IntegrationTest(ITestOutputHelper output)
        {
            _output = output;
            dynamic config = ConfigurationLoader.LoadConfiguration("secrets.json");

            //AWS
            dynamic aws = config.aws;
            string awsId = aws.id;
            string awsKey = aws.key;
            string awsRegion = aws.region;
            awsClient = BucketClientFactory.CreateClient(CloudServiceProvider.AWS, new AWSCredential(awsId, awsKey, awsRegion));
            awsTargetBucket = aws.bucket;

            //DO
            dynamic DO = config.DO;
            string doId = DO.id;
            string doKey = DO.key;
            string doRegion = DO.region;
            doClient = BucketClientFactory.CreateClient(CloudServiceProvider.DigitalOcean, new DigitalOceanCredential(doId, doKey, doRegion));
            doTargetBucket = DO.bucket;

            //Azure
            dynamic azure = config.azure;
            string azureId = azure.id;
            string azurekey = azure.key;
            azureClient = BucketClientFactory.CreateClient(CloudServiceProvider.Azure, new AzureCredential(azureId, azurekey));
            azureTargetBucket = azure.bucket;


        }

        [Fact]
        public async void AWSCrud()
        {
            var client = awsClient;
            var buc = awsTargetBucket;
            int test = 0;
            //Bucket should not exist
            var bucketShouldNotExist1 = await client.ExistBucket(buc);
            Assert.False(bucketShouldNotExist1);
            _output.WriteLine("Passed test" + test++); //0

            //Create bucket
            var resp = await client.CreateBucket(buc);
            Assert.True(resp.Success);
            _output.WriteLine("Passed test" + test++); //1

            //Bucket should ow exist!
            var bucketShouldExist1 = await client.ExistBucket(buc);
            Assert.True(bucketShouldExist1);
            _output.WriteLine("Passed test" + test++); //2

            //Should be able to set public policy
            var publicResp = await client.SetReadPolicy(buc, ReadAccess.Public);
            Assert.True(publicResp.Success);
            _output.WriteLine("Passed test" + test++); //3

            byte[] blob = ConfigurationLoader.LoadBlobAsBytes("sophie.png");
            IBucket bucket = await client.GetBucket(buc);

            //Bucket should not exist
            var blobExist1 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist1);
            _output.WriteLine("Passed test" + test++); //4

            //Should fail to update non-existing blob
            var failUpdate = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.False(failUpdate.Success);
            _output.WriteLine("Passed test" + test++); //5

            //Should successfully put non-existing blob
            var passPut1 = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(passPut1.Success);
            _output.WriteLine("Passed test" + test++); //6

            //Should fail to create exist blob
            var create1 = await bucket.CreateBlob(blob, "sophie.png");
            Assert.False(create1.Success);
            _output.WriteLine("Passed test" + test++); //7

            //Should successfully delete blob using URI
            var del1 = await bucket.DeleteBlob(passPut1.Usable);
            Assert.True(del1.Success);
            _output.WriteLine("Passed test" + test++);//8

            //Should successfully create blob
            var createResp = await bucket.CreateBlob(blob, "sophie.png");
            Assert.True(createResp.Success);
            _output.WriteLine("Passed test" + test++); //9

            //Blob should exist        
            var blobExist2 = await bucket.ExistBlob("sophie.png");
            Assert.True(blobExist2);
            _output.WriteLine("Passed test" + test++); //10

            //Should be able to update blob using uri
            var updateURI = await bucket.UpdateBlob(blob, createResp.Usable);
            Assert.True(updateURI.Success);
            _output.WriteLine("Passed test" + test++); //11

            //should be able to update blob using key
            var updateKey = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.True(updateKey.Success);
            _output.WriteLine("Passed test" + test++); //12

            //Should be able to put blob using uri
            var putURI = await bucket.PutBlob(blob, createResp.Usable);
            Assert.True(putURI.Success);
            _output.WriteLine("Passed test" + test++); //13

            //Should be able to update blob using uri
            var putKey = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(putKey.Success);
            _output.WriteLine("Passed test" + test++); //14

            //Should be able to delete blob using key
            var delBlob = await bucket.DeleteBlob("sophie.png");
            Assert.True(delBlob.Success);
            _output.WriteLine("Passed test" + test++); //15

            //Should not exist with uri
            var blobExist3 = await bucket.ExistBlob(createResp.Usable);
            Assert.False(blobExist3);
            _output.WriteLine("Passed test" + test++); //16

            //Should not exist with key
            var blobExist4 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist4);
            _output.WriteLine("Passed test" + test++);//17

            //Should be able to set public private
            var privateBucket = await client.SetReadPolicy(buc, ReadAccess.Private);
            Assert.True(privateBucket.Success);
            _output.WriteLine("Passed test" + test++, "Private Policy set!"); //18

            //Should be able to delete bucket
            var delBucket = await client.DeleteBucket(buc);
            Assert.True(delBucket.Success);
            _output.WriteLine("Passed test" + test++);//19

            //Bucket should no longer exist
            var existBucket2 = await client.ExistBucket(buc);
            Assert.False(existBucket2);
            _output.WriteLine("Passed test" + test++); //20


        }
        [Fact]
        public async void AzureCrud()
        {
            var client = azureClient;
            var buc = azureTargetBucket;

            _output.WriteLine(client.ToString() + buc);
            int test = 0;
            //Bucket should not exist
            var bucketShouldNotExist1 = await client.ExistBucket(buc);

            Assert.False(bucketShouldNotExist1);
            _output.WriteLine("Passed test" + test++); //0

            //Create bucket
            var resp = await client.CreateBucket(buc);
            _output.WriteLine(resp.StatusCode.ToString());
            _output.WriteLine(resp.Message);
            Assert.True(resp.Success);
            _output.WriteLine("Passed test" + test++); //1

            //Bucket should ow exist!
            var bucketShouldExist1 = await client.ExistBucket(buc);
            Assert.True(bucketShouldExist1);
            _output.WriteLine("Passed test" + test++); //2

            //Should be able to set public policy
            var publicResp = await client.SetReadPolicy(buc, ReadAccess.Public);
            Assert.True(publicResp.Success);
            _output.WriteLine("Passed test" + test++); //3

            byte[] blob = ConfigurationLoader.LoadBlobAsBytes("sophie.png");
            IBucket bucket = await client.GetBucket(buc);

            //Bucket should not exist
            var blobExist1 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist1);
            _output.WriteLine("Passed test" + test++); //4

            //Should fail to update non-existing blob
            var failUpdate = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.False(failUpdate.Success);
            _output.WriteLine("Passed test" + test++); //5

            //Should successfully put non-existing blob
            var passPut1 = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(passPut1.Success);
            _output.WriteLine("Passed test" + test++); //6

            //Should fail to create exist blob
            var create1 = await bucket.CreateBlob(blob, "sophie.png");
            Assert.False(create1.Success);
            _output.WriteLine("Passed test" + test++); //7

            //Should successfully delete blob using URI
            var del1 = await bucket.DeleteBlob(passPut1.Usable);
            Assert.True(del1.Success);
            _output.WriteLine("Passed test" + test++);//8

            //Should successfully create blob
            var createResp = await bucket.CreateBlob(blob, "sophie.png");
            Assert.True(createResp.Success);
            _output.WriteLine("Passed test" + test++); //9

            //Blob should exist        
            var blobExist2 = await bucket.ExistBlob("sophie.png");
            Assert.True(blobExist2);
            _output.WriteLine("Passed test" + test++); //10

            //Should be able to update blob using uri
            var updateURI = await bucket.UpdateBlob(blob, createResp.Usable);
            Assert.True(updateURI.Success);
            _output.WriteLine("Passed test" + test++); //11

            //should be able to update blob using key
            var updateKey = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.True(updateKey.Success);
            _output.WriteLine("Passed test" + test++); //12

            //Should be able to put blob using uri
            var putURI = await bucket.PutBlob(blob, createResp.Usable);
            Assert.True(putURI.Success);
            _output.WriteLine("Passed test" + test++); //13

            //Should be able to update blob using uri
            var putKey = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(putKey.Success);
            _output.WriteLine("Passed test" + test++); //14

            //Should be able to delete blob using key
            var delBlob = await bucket.DeleteBlob("sophie.png");
            Assert.True(delBlob.Success);
            _output.WriteLine("Passed test" + test++); //15

            //Should not exist with uri
            var blobExist3 = await bucket.ExistBlob(createResp.Usable);
            Assert.False(blobExist3);
            _output.WriteLine("Passed test" + test++); //16

            //Should not exist with key
            var blobExist4 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist4);
            _output.WriteLine("Passed test" + test++);//17

            //Should be able to set public private
            var privateBucket = await client.SetReadPolicy(buc, ReadAccess.Private);
            Assert.True(privateBucket.Success);
            _output.WriteLine("Passed test" + test++, "Private Policy set!"); //18

            //Should be able to delete bucket
            var delBucket = await client.DeleteBucket(buc);
            Assert.True(delBucket.Success);
            _output.WriteLine("Passed test" + test++);//19

            //Bucket should no longer exist
            var existBucket2 = await client.ExistBucket(buc);
            Assert.False(existBucket2);
            _output.WriteLine("Passed test" + test++); //20


        }
        [Fact]
        public async void DigitalOceanCrud()
        {
            var client = doClient;
            var buc = doTargetBucket;

            _output.WriteLine(client.ToString() + buc);
            int test = 0;
            //Bucket should not exist
            var bucketShouldNotExist1 = await client.ExistBucket(buc);

            Assert.False(bucketShouldNotExist1);
            _output.WriteLine("Passed test" + test++); //0

            //Create bucket
            var resp = await client.CreateBucket(buc);
            _output.WriteLine(resp.StatusCode.ToString());
            _output.WriteLine(resp.Message);
            Assert.True(resp.Success);
            _output.WriteLine("Passed test" + test++); //1

            //Bucket should ow exist!
            var bucketShouldExist1 = await client.ExistBucket(buc);
            Assert.True(bucketShouldExist1);
            _output.WriteLine("Passed test" + test++); //2

            //Should be able to set public policy
            var publicResp = await client.SetReadPolicy(buc, ReadAccess.Public);
            Assert.True(publicResp.Success);
            _output.WriteLine("Passed test" + test++); //3

            byte[] blob = ConfigurationLoader.LoadBlobAsBytes("sophie.png");
            IBucket bucket = await client.GetBucket(buc);

            //Bucket should not exist
            var blobExist1 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist1);
            _output.WriteLine("Passed test" + test++); //4

            //Should fail to update non-existing blob
            var failUpdate = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.False(failUpdate.Success);
            _output.WriteLine("Passed test" + test++); //5

            //Should successfully put non-existing blob
            var passPut1 = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(passPut1.Success);
            _output.WriteLine("Passed test" + test++); //6

            //Should fail to create exist blob
            var create1 = await bucket.CreateBlob(blob, "sophie.png");
            Assert.False(create1.Success);
            _output.WriteLine("Passed test" + test++); //7

            //Should successfully delete blob using URI
            var del1 = await bucket.DeleteBlob(passPut1.Usable);
            Assert.True(del1.Success);
            _output.WriteLine("Passed test" + test++);//8

            //Should successfully create blob
            var createResp = await bucket.CreateBlob(blob, "sophie.png");
            Assert.True(createResp.Success);
            _output.WriteLine("Passed test" + test++); //9

            //Blob should exist        
            var blobExist2 = await bucket.ExistBlob("sophie.png");
            Assert.True(blobExist2);
            _output.WriteLine("Passed test" + test++); //10

            //Should be able to update blob using uri
            var updateURI = await bucket.UpdateBlob(blob, createResp.Usable);
            Assert.True(updateURI.Success);
            _output.WriteLine("Passed test" + test++); //11

            //should be able to update blob using key
            var updateKey = await bucket.UpdateBlob(blob, "sophie.png");
            Assert.True(updateKey.Success);
            _output.WriteLine("Passed test" + test++); //12

            //Should be able to put blob using uri
            var putURI = await bucket.PutBlob(blob, createResp.Usable);
            Assert.True(putURI.Success);
            _output.WriteLine("Passed test" + test++); //13

            //Should be able to update blob using uri
            var putKey = await bucket.PutBlob(blob, "sophie.png");
            Assert.True(putKey.Success);
            _output.WriteLine("Passed test" + test++); //14

            //Should be able to delete blob using key
            var delBlob = await bucket.DeleteBlob("sophie.png");
            Assert.True(delBlob.Success);
            _output.WriteLine("Passed test" + test++); //15

            //Should not exist with uri
            var blobExist3 = await bucket.ExistBlob(createResp.Usable);
            Assert.False(blobExist3);
            _output.WriteLine("Passed test" + test++); //16

            //Should not exist with key
            var blobExist4 = await bucket.ExistBlob("sophie.png");
            Assert.False(blobExist4);
            _output.WriteLine("Passed test" + test++);//17

            //Should be able to set public private
            var privateBucket = await client.SetReadPolicy(buc, ReadAccess.Private);
            Assert.True(privateBucket.Success);
            _output.WriteLine("Passed test" + test++, "Private Policy set!"); //18

            //Should be able to delete bucket
            var delBucket = await client.DeleteBucket(buc);
            Assert.True(delBucket.Success);
            _output.WriteLine("Passed test" + test++);//19

            //Bucket should no longer exist
            var existBucket2 = await client.ExistBucket(buc);
            Assert.False(existBucket2);
            _output.WriteLine("Passed test" + test++); //20


        }
    }
}
