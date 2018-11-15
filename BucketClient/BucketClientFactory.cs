using BucketClient.AWS;
using BucketClient.Azure;
using BucketClient.DigitalOcean;
using BucketClient.Library.Credentials;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BucketClient
{
    public class BucketClientFactory
    {
        public static IBucketClient CreateClient(ServiceProvider service, ICredential credential)
        {
            HttpClient httpClient = new HttpClient();

            switch (service)
            {
                case ServiceProvider.AWS:
                    if (!(credential is AWSCredential)) throw new ArgumentException("AWS needs AWSCredential!");
                    AWSCredential aws = credential as AWSCredential;
                    return new AWSBucketClient(httpClient, aws.accessKeyID, aws.accessKeySecret, aws.region);
                case ServiceProvider.Azure:
                    if (!(credential is AzureCredential)) throw new ArgumentException("Azure needs AzureCredential!");
                    AzureCredential azure = credential as AzureCredential;
                    return new AzureBucketClient(azure.AccountName, azure.Secret);
                case ServiceProvider.Google:
                    throw new NotImplementedException();
                case ServiceProvider.DigitalOcean:
                    if (!(credential is DigitalOceanCredential)) throw new ArgumentException("Digital Ocean needs DigitalOceanCredential!");
                    DigitalOceanCredential DO = credential as DigitalOceanCredential;
                    return new DigitalOceanBucketClient(httpClient, DO.accessKeyID, DO.accessKeySecret, DO.region);
                case ServiceProvider.AliCloud:
                    throw new NotImplementedException();
            }
            return null;
        }
    }

    public enum ServiceProvider
    {
        AWS,
        Azure,
        Google,
        DigitalOcean,
        AliCloud
    }

    public interface ICredential
    {

    } 

    
}
