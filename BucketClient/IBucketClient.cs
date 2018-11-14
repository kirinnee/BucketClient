using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BucketClient
{
    public interface IBucketClient
    {

        #region BUCKET

        /// <summary>
        /// Check if bucket exist
        /// </summary>
        /// <param name="key">ID of bucket</param>
        /// <returns></returns>
        Task<bool> ExistBucket(string key);

        /// <summary>
        /// Create a bucket with the existing key. Will ignore if bucket already exist
        /// </summary>
        /// <param name="key">ID of bucket</param>
        /// <returns></returns>
        Task<OperationResult> CreateBucket(string key);

        /// <summary>
        /// Delete the bucket with ID
        /// </summary>
        /// <param name="key">Delete bucket with exist key</param>
        /// <returns></returns>
        Task<OperationResult> DeleteBucket(string key);

        /// <summary>
        /// Sets the read access policy of the 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        Task<OperationResult> SetReadPolicy(string key, ReadAccess access);

        /// <summary>
        /// Returns bucket instance if exist. 
        /// Returns null if it does not exist.
        /// </summary>
        /// <param name="key">bucket id</param>
        /// <returns></returns>
        Task<IBucket> GetBucket(string key);


        /// <summary>
        /// Returns a virtual bucket, whether it exist or not. Its faster, but it may result in 400 or 404 if bucket does not exist
        /// </summary>
        /// <param name="key">Bucket</param>
        /// <returns></returns>
        Task<IBucket> UnsafeGetBucket(string key);

        #endregion


        #region BLOB

        /// <summary>
        /// Check if a blob with the URI exist
        /// </summary>
        /// <param name="key">the URI of the blob</param>
        /// <returns></returns>
        Task<bool> ExistBlob(Uri key);

        /// <summary>
        /// Update the blob with byte array.
        /// Returns 404 if blob does not exist
        /// </summary>
        /// <param name="payload">new blob binary</param>
        /// <param name="key"></param>
        Task<OperationResult> UpdateBlob(byte[] payload, Uri key);

        /// <summary>
        /// Update the blob with stream
        /// Returns 404 if blob does not exist
        /// </summary>
        /// <param name="payload">new blob binary</param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<OperationResult> UpdateBlob(Stream payload, Uri key);

        /// <summary>
        /// Update the blob with the new byte array, whether it exist or not
        /// </summary>
        /// <param name="payload">new blob binary</param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(byte[] payload, Uri key);

        /// <summary>
        /// Update the blob with stream, whether it exist or not
        /// </summary>
        /// <param name="payload">new blob binary</param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(Stream payload, Uri key);

        /// <summary>
        /// Delete the blob of the URI
        /// </summary>
        /// <param name="key">URI of the blob to delete</param>
        /// <returns></returns>
        Task<OperationResult> DeleteBlob(Uri key);


        #endregion

    }

}
