# BucketClient

A netstandard class Library that allows programmers to use 1 interface for all different cloud service's storage bucket service.
It has simplified functionalities, which includes:
- Bucket CRUD
- Blob CRUD
- Bucket level ReadAccess control

Useful for CLI for deployment, or multiple-blob storage projec that uses same interface.

# Getting Started

Install via .NET CLI
```powershell
$ dotnet add package Kirinnee.StorageBucketClient 
```

or 

Install via NuGet Package Manager
```powershell
PM> Install-Package Kirinnee.StorageBucketClient 
```


# Usage

## Bucket Client Creation

The only difference between using each service is the initial creation of the client. Using each services' own authentication method, you can create the same client, which are used the same later.

### Amazon Web Service S3 Bucket
```cs
//Create a credential object with your secrets! (passed in via env var or something)
string accessId = "someID";
string accessSecret = "supersecret!";
string region = "us-east-1";
ICredential awsCred = new AWSCredential(accessId, accessSecret, region);

//Use the BucketClientFactory to create a bucket Client
IBucketClient bucketClient = BucketClientFactory.CreateClient(CloudServiceProvider.AWS, awsCred);

//Now you can use the bucket client as it is
```

### Digital Ocean Spaces
```cs
//Create a credential object with your secrets! (passed in via env var or something)
string accessId = "someID";
string accessSecret = "supersecret!";
string region = "nyc3";
ICredential digitalOceanCred = new DigitalOceanCredential(accessId, accessSecret, region);

//Use the BucketClientFactory to create a bucket Client
IBucketClient bucketClient = BucketClientFactory.CreateClient(CloudServiceProvider.DigitalOcean, digitalOceanCred);

//Now you can use the bucket client as it is
```

### Microsoft Azure Blob Storage
```cs
//Create a credential object with your secrets! (passed in via env var or something)
string accountName = "storage-account-name";
string accessToken = "supersecret!";
ICredential azureCred = new AzureCredential(accessId, accessSecret);

//Use the BucketClientFactory to create a bucket Client
IBucketClient bucketClient = BucketClientFactory.CreateClient(CloudServiceProvider.Azure,azureCred);

//Now you can use the bucket client as it is
```

### Google Cloud Platform Cloud Storage
```cs
//Create a credential object with your secrets
string projectID = "project-id-123456";
string jsonSecret = "{\"secret\":\"really-super-secret\"}";//download the json secret and pass the json as a string, somehow
ICredential googleCred = new GCPCredential(projectID, jsonSecret);

//Use the BucketClientFactory to create a bucket Client
IBucketClient bucketClient = BucketClientFactory.CreateClient(CloudServiceProvider.GCP,googleCred);

//Now you can use the bucket client as it is
```

## OperationResult

Almost all methods provided in this library returns an Operation Result, which contains 4 key values that is used to inform you about the request to the bucket/blob service.


| Property  |  Type |  Description  |
|---|---|---|
| Success  | boolean  | Whether the operation is successful or not |
| Message  |  string | Response message to provide more information on how the operation went  |
| Code  | HttpStatusCode  | The Real HttpStatusCode recieved (If sucess is not good enough) |
| Usable| Uri | Usable link to the blob or file

## Bucket CRUD

### Creating Bucket
Creates a bucket
```cs
IBucketClient client = _bucketClient;
OperationResult result = await client.CreateBucket("bucket-name");
```

### Deleting Bucket
Deletes a bucket
```cs
IBucketClient client = _bucketClient;
OperationResult result = await client.Delete("bucket-name");
```

### Making bucket public-read
Makes the bucket and all its blob/content/objects readable/accessible by everyone
```cs
IBucketClient client = _bucketClient;
OperationResult result = await client.SetReadPolicy("bucket-name", ReadAccess.Public);
```

### Making bucket private (only owner has access)
Makes the bucket and all its blob/content/objects readable/accessible only by owner (with the credential)
```cs
IBucketClient client = _bucketClient;
OperationResult result = await client.SetReadPolicy("bucket-name", ReadAccess.Private);
```

### Checking if bucket exist
Checks if the bucket exist
```cs
IBucketClient client = _bucketClient;
bool exist = await client.ExistBucket("bucket-name");
```

### Get Bucket
Gets a bucket object, which can be used to perform CRUD operation on the objects it contains.
The object will be IBucket
```cs
IBucket bucket = await client.GetBucket("bucket-name");
```


## Blob CRUD with IBucket

### Creating a Blob
Creates a blob in the bucket. MimeType will be automatically detected and set. 

Access Level is inherited from bucket.

Will fail if blob already exist.

```cs
IBucket bucket = somebucket;

//Create a blob using byte array
byte[] bytePayload = Encoding.ASCII.GetBytes(someImageString);
OperationResult result1 = await bucket.CreateBlob(bytePayload, "image.png");

//Create a blob using Stream
FileStream streamPayload = File.ReadFileStream(someImagePath);
OperationResult result2 = await bucket.CreateBlob(streamPayload, "image.png");
```

### Updating a Blob
Updates an exist blob in the bucket. MimeType will be automatically updated too.

Will fail if blob does not exist

```cs
IBucket bucket = somebucket;

byte[] bytePayload = Encoding.ASCII.GetBytes(someImageString);
FileStream streamPayload = File.ReadFileStream(someImagePath);

//Update the blob using byte array with the bucket specific id
OperationResult result1 = await bucket.CreateBlob(bytePayload, "image.png");

//Update the blob using Stream with the bucket specific id
OperationResult result2 = await bucket.CreateBlob(streamPayload, "image.png");

//Update the blob using byte array with the full blob URI
OperationResult result3 = await bucket.CreateBlob(bytePayload, "https://aws.com/somebucket/image.png");

//Update the blob using byte array with the full blob URI
OperationResult result4 = await bucket.CreateBlob(streamPayload, "https://aws.com/somebucket/image.png");
```

### Putting a Blob
This will replace the current space with the nwe blob, whether it exist or not.

MimeType will be automatically set.
```cs
IBucket bucket = somebucket;

byte[] bytePayload = Encoding.ASCII.GetBytes(someImageString);
FileStream streamPayload = File.ReadFileStream(someImagePath);

//Update the blob using byte array with the bucket specific id
OperationResult result1 = await bucket.PutBlob(bytePayload, "image.png");

//Update the blob using Stream with the bucket specific id
OperationResult result2 = await bucket.PutBlob(streamPayload, "image.png");

//Update the blob using byte array with the full blob URI
OperationResult result3 = await bucket.PutBlob(bytePayload, "https://aws.com/somebucket/image.png");

//Update the blob using byte array with the full blob URI
OperationResult result4 = await bucket.PutBlob(streamPayload, "https://aws.com/somebucket/image.png");
```

### Check if a blob Exist
```cs
IBucket bucket = somebucket;

//Check if the blob exist using the bucket specific id
bool exist1 = await bucket.ExistBlob("image.png");

//Check if the blob exist usign the full blob URI
bool exist2 = await bucket.ExistBlob("https://aws.com/somebucket/image.png");
```

## Blob CRUD With IBucketClient

### Updating a Blob
Updates an exist blob in the bucket. MimeType will be automatically updated too.

Will fail if blob does not exist

```cs
IBucketClient client = _bucketClient;

byte[] bytePayload = Encoding.ASCII.GetBytes(someImageString);
FileStream streamPayload = File.ReadFileStream(someImagePath);

//Update the blob using byte array with the full blob URI
OperationResult result3 = await client.CreateBlob(bytePayload, "https://aws.com/somebucket/image.png");

//Update the blob using byte array with the full blob URI
OperationResult result4 = await client.CreateBlob(streamPayload, "https://aws.com/somebucket/image.png");
```

### Putting a Blob
This will replace the current space with the nwe blob, whether it exist or not.

MimeType will be automatically set.
```cs
IBucketClient client = _bucketClient;

byte[] bytePayload = Encoding.ASCII.GetBytes(someImageString);
FileStream streamPayload = File.ReadFileStream(someImagePath);

//Update the blob using byte array with the full blob URI
OperationResult result3 = await client.PutBlob(bytePayload, "https://aws.com/somebucket/image.png");

//Update the blob using byte array with the full blob URI
OperationResult result4 = await client.PutBlob(streamPayload, "https://aws.com/somebucket/image.png");
```

### Check if a blob Exist
```cs
IBucketClient client = _bucketClient;

//Check if the blob exist usign the full blob URI
bool exist2 = await client.ExistBlob("https://aws.com/somebucket/image.png");
```


## Contributing
Please read [CONTRIBUTING.md](CONTRIBUTING.MD) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning 
We use [SemVer](https://semver.org/) for versioning. For the versions available, see the tags on this repository.

## Authors
* [kirinnee](mailto:kirinnee@gmail.com) 

## License
This project is licensed under MIT - see the [LICENSE.md](LICENSE.MD) file for details