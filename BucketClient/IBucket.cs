using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BucketClient
{
    
    public interface IBucket
    {

        /// <summary>
        /// Returns the uri of the blob with a given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Uri> GetUri(string key);
        
        #region EXIST

        /// <summary>
        /// Check if blob exist
        /// </summary>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<bool> ExistBlob(string key);
        /// <summary>
        /// Check if blob exist
        /// </summary>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<bool> ExistBlob(Uri key);

        #endregion

        #region CREATE

        /// <summary>
        /// Create a Blob, returns 400 if it doesn't exist
        /// </summary>
        /// <param name="payload">the binary in bytes of the Blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> CreateBlob(byte[] payload, string key);

        /// <summary>
        /// Create a Blob, returns 400 if it doesn't exist
        /// </summary>
        /// <param name="payload">file stream of the Blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> CreateBlob(Stream payload, string key);

        #endregion

        #region Update

        /// <summary>
        /// Updates a blob, using byte array returns 404 if it doesn't exist
        /// </summary>
        /// <param name="payload">new binary in bytes of the blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> UpdateBlob(byte[] payload, string key);
        /// <summary>
        /// Updates a blob, using byte array returns 404 if it doesn't exist
        /// </summary>
        /// <param name="payload">new binary in bytes of the blob</param>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<OperationResult> UpdateBlob(byte[] payload, Uri key);

        /// <summary>
        /// Updates a blob, using a stream
        /// Returns 404 if it doesn't exist
        /// </summary>
        /// <param name="payload">file stream of the Blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> UpdateBlob(Stream payload, string key);
        /// <summary>
        /// Updates a blob, using a stream
        /// Returns 404 if it doesn't exist
        /// </summary>
        /// <param name="payload">file stream of the Blob</param>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<OperationResult> UpdateBlob(Stream payload, Uri key);

        #endregion

        #region Put

        /// <summary>
        /// Puts a blob, whether it exist or not
        /// </summary>
        /// <param name="payload">new binary in bytes of the blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(byte[] payload, string key);
        /// <summary>
        /// Puts a blob, whether it exist or not
        /// </summary>
        /// <param name="payload">new binary in bytes of the blob</param>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(byte[] payload, Uri key);

        /// <summary>
        /// Puts a blob, whether it exist or not
        /// </summary>
        /// <param name="payload">file stream of the Blob</param>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(Stream payload, string key);
        /// <summary>
        /// Puts a blob, whether it exist or not
        /// </summary>
        /// <param name="payload">file stream of the Blob</param>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<OperationResult> PutBlob(Stream payload, Uri key);

        #endregion

        #region

        /// <summary>
        /// Deletes the blob
        /// </summary>
        /// <param name="key">ID of the blob</param>
        /// <returns></returns>
        Task<OperationResult> DeleteBlob(string key);

        /// <summary>
        /// Deletes the blob
        /// </summary>
        /// <param name="key">URI of the blob</param>
        /// <returns></returns>
        Task<OperationResult> DeleteBlob(Uri key);

        #endregion
    }
}
