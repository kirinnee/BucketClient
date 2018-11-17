# Cross-Service Bucket Client

Use the repository pattern to encapulate different Cloud Service provider and normalize them to same few methods.

Features include:
- Bucket CRUD
- Blob CRUD
- Private or Public Read Access on Bucket Level

Compatible Cloud Services:
- Amazon Web Service S3 Buckets
- Azure Blob Storage
- DigitalOcean Space
- Google Cloud Platform Cloud Storage

# Documentation

1. Invoke the client using your platform's credential
    ```cs
    //Create a credential 
    string accessId = "someID";
    string accessSecret = "supersecret!";
    string region = "us-east-1";
    ICredential awsCred = new AWSCredential(accessId, accessSecret, region);

    //Use the BucketClientFactory to create a bucket Client
    IBucketClient bucketClient = BucketClientFactory.CreateClient(CloudServiceProvider.AWS, awsCred);
    ```   
2. Use the bucketClient to get bucket or perform CRUD Actions

The Full Documentation will not be placed here because Nuget has a 8000 byte limit on documentation :<

The documentation isn't that long...

[Full Documentation](https://github.com/kirinnee/BucketClient#bucketclient)