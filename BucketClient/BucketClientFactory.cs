using BucketClient.AWS;
using BucketClient.Azure;
using BucketClient.DigitalOcean;
using BucketClient.GCP;
using BucketClient.Library.Credentials;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BucketClient
{
    public class BucketClientFactory
    {
        public static IBucketClient CreateClient(CloudServiceProvider service, ICredential credential)
        {
            HttpClient httpClient = new HttpClient();
            switch (service)
            {
                case CloudServiceProvider.AWS:
                    if (!(credential is AWSCredential)) throw new ArgumentException("AWS needs AWSCredential!");
                    AWSCredential aws = credential as AWSCredential;
                    return new AWSBucketClient(httpClient, aws.accessKeyID, aws.accessKeySecret, aws.region);
                case CloudServiceProvider.Azure:
                    if (!(credential is AzureCredential)) throw new ArgumentException("Azure needs AzureCredential!");
                    AzureCredential azure = credential as AzureCredential;
                    return new AzureBucketClient(azure.AccountName, azure.Secret);
                case CloudServiceProvider.GCP:
                    if (!(credential is GCPCredential)) throw new ArgumentException("Google Cloud Platform needs GCPCredential!");
                    GCPCredential gcp = credential as GCPCredential;
                    return new GCPBucketClient(gcp.projectID, gcp.secretJSON);
                case CloudServiceProvider.DigitalOcean:
                    if (!(credential is DigitalOceanCredential)) throw new ArgumentException("Digital Ocean needs DigitalOceanCredential!");
                    DigitalOceanCredential DO = credential as DigitalOceanCredential;
                    return new DigitalOceanBucketClient(httpClient, DO.accessKeyID, DO.accessKeySecret, DO.region);
                case CloudServiceProvider.AliCloud:
                    throw new NotImplementedException();
            }
            return null;
        }
    }

    public enum CloudServiceProvider
    {
        AWS,
        Azure,
        GCP,
        DigitalOcean,
        AliCloud
    }

    public interface ICredential
    {

    } 

    
}
