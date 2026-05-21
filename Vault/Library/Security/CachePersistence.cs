// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Azure.Identity;

namespace Microsoft.Vault.Library
{
    public class CachePersistence
    {
        public string FileName { get; private set; }
        private static readonly object FileLock = new object();
        private static readonly RandomNumberGenerator RNGCryp = RandomNumberGenerator.Create();
        private const int EntropySize = 16;
        const string TOKEN_CACHE_NAME = "MyTokenCache";

        public CachePersistence(string domainHint)
        {
            FileName = Environment.ExpandEnvironmentVariables(string.Format(Consts.VaultTokenCacheFileName, domainHint));
        }
        /// <summary>
        /// Initializes the cache against a local file.
        /// If the file is already present, it loads its content in the MSAL cache
        /// </summary>
        /// <param name="domainHint">For example: microsoft.com or gme.gbl</param>
        public CachePersistence(string domainHint, AuthenticationRecord authRecord)
        {
            FileName = Environment.ExpandEnvironmentVariables(string.Format(Consts.VaultTokenCacheFileName, domainHint));
            Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);
            AfterAccessNotification(authRecord);
        }

        /// <summary>
        /// Gets all login names for which there is a token cached locally.
        /// </summary>
        public static string[] GetAllFileTokenCacheLoginNames()
        {
            string[] paths = Directory.GetFiles(Environment.ExpandEnvironmentVariables(Consts.VaultTokenCacheDirectory));
            for (int i = 0; i < paths.Length; i++)
            {
                //Gets filename from path.
                paths[i] = paths[i].Split('\\').Last();

                //Gets login name from filename.
                paths[i] = paths[i].Split('_')[1];
            }
            return paths;
        }

        /// <summary>
        /// Empties all persistent stores.
        /// </summary>
        public static void ClearAllFileTokenCaches()
        {
            string[] tokenNames = GetAllFileTokenCacheLoginNames();
            foreach (string token in tokenNames)
            {
                new CachePersistence(token).Clear();
            }
        }

        /// <summary>
        /// Empties the persistent store
        /// </summary>
        public void Clear()
        {
            File.Delete(FileName);
        }

        /// <summary>
        /// Renames the cache.
        /// </summary>
        /// <param name="newName"></param>
        public void Rename(string newName, AuthenticationRecord authRecord)
        {
            newName = Environment.ExpandEnvironmentVariables(string.Format(Consts.VaultTokenCacheFileName, newName));
            if (File.Exists(newName))
            {
                File.Delete(newName);
            }
            File.Move(FileName, newName);
            FileName = newName;
        }

        void BeforeAccessNotification(AuthenticationRecord authRecord)
        {
            lock (FileLock)
            {
                if (!File.Exists(FileName))
                    return;

                var fileBytes = File.ReadAllBytes(FileName);
                if (fileBytes.Length <= EntropySize)
                    return;

                // Entropy is stored as the first 16 bytes of the file
                var entropy = new byte[EntropySize];
                Array.Copy(fileBytes, 0, entropy, 0, EntropySize);

                var encryptedData = new byte[fileBytes.Length - EntropySize];
                Array.Copy(fileBytes, EntropySize, encryptedData, 0, encryptedData.Length);

                var decryptedData = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
                using var memoryStream = new MemoryStream(decryptedData);
                authRecord = AuthenticationRecord.Deserialize(memoryStream);
            }
        }

        void AfterAccessNotification(AuthenticationRecord authRecord)
        {
            lock (FileLock)
            {
                var credential = new InteractiveBrowserCredential(
                    new InteractiveBrowserCredentialOptions { TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = TOKEN_CACHE_NAME } });

                authRecord = credential.Authenticate();

                var toEncrypt = Encoding.UTF8.GetBytes(authRecord.ToString());

                // Generate random entropy and persist it alongside encrypted data
                var entropy = new byte[EntropySize];
                RNGCryp.GetBytes(entropy);

                var encryptedData = ProtectedData.Protect(toEncrypt, entropy, DataProtectionScope.CurrentUser);

                // Write entropy + encrypted data atomically
                using var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                fs.Write(entropy, 0, entropy.Length);
                fs.Write(encryptedData, 0, encryptedData.Length);
            }
        }

        public static int EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream S)
        {
            if (Buffer == null)
                throw new ArgumentNullException("Buffer");
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");
            if (S == null)
                throw new ArgumentNullException("S");

            int length = 0;

            // Encrypt the data and store the result in a new byte array. The original data remains unchanged.
            byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

            // Write the encrypted data to a stream.
            if (S.CanWrite && encryptedData != null)
            {
                S.Write(encryptedData, 0, encryptedData.Length);

                length = encryptedData.Length;
            }

            // Return the length that was written to the stream.
            return length;
        }

        public static byte[] DecryptDataFromStream(byte[] Entropy, DataProtectionScope Scope, Stream S, int Length)
        {
            if (S == null)
                throw new ArgumentNullException("S");
            if (Length <= 0)
                throw new ArgumentException("Length");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");

            byte[] inBuffer = new byte[Length];
            byte[] outBuffer;

            // Read the encrypted data from a stream.
            if (S.CanRead)
            {
                S.ReadExactly(inBuffer, 0, Length);

                outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, Scope);
            }
            else
            {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream.
            return outBuffer;
        }
    }
}
