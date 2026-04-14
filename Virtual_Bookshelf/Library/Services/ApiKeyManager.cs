using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Virtual_Bookshelf.Library.Services
{
    public static class ApiKeyManager
    {
        private static readonly string StorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualBookshelf");
        private static readonly string ApiKeyFilePath = Path.Combine(StorageDirectory, "apikey.dat");

        private const int KeySize = 32; // 256 bits
        private const int IvSize = 16; // 128 bits
        private const int SaltSize = 16;
        private const int Iterations = 100_000;

        public static string GetOrCreateApiKey()
        {
            if (File.Exists(ApiKeyFilePath))
            {
                try
                {
                    var encryptedData = File.ReadAllBytes(ApiKeyFilePath);
                    var key = DecryptApiKey(encryptedData);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        return key;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load saved API key: {ex.Message}");
                }
            }

            return PromptForApiKey();
        }

        public static string GetApiKey()
        {
            if (File.Exists(ApiKeyFilePath))
            {
                try
                {
                    var encryptedData = File.ReadAllBytes(ApiKeyFilePath);
                    var key = DecryptApiKey(encryptedData);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        return key;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load saved API key: {ex.Message}");
                }
            }

            return string.Empty;
        }

        private static string PromptForApiKey()
        {
            Console.WriteLine("Google Books API key is required to use book search features.");
            string apiKey;
            do
            {
                Console.Write("Enter your Google Books API key: ");
                apiKey = Console.ReadLine()?.Trim() ?? string.Empty;
            } while (string.IsNullOrWhiteSpace(apiKey));

            SaveApiKey(apiKey);
            return apiKey;
        }

        public static void SaveApiKey(string apiKey)
        {
            try
            {
                Directory.CreateDirectory(StorageDirectory);
                var encryptedBytes = EncryptApiKey(apiKey);
                File.WriteAllBytes(ApiKeyFilePath, encryptedBytes);
                Console.WriteLine("API key saved securely.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save API key securely: {ex.Message}");
            }
        }

        private static string GetEncryptionPassphrase()
        {
            // Machine-specific fallback passphrase derived from user+machine identity
            return (Environment.UserName + "@" + Environment.MachineName).ToLowerInvariant();
        }

        private static byte[] EncryptApiKey(string apiKey)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var iv = RandomNumberGenerator.GetBytes(IvSize);
            var key = DeriveKey(GetEncryptionPassphrase(), salt);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(apiKey);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length);
            ms.Write(iv, 0, iv.Length);
            ms.Write(cipherBytes, 0, cipherBytes.Length);

            return ms.ToArray();
        }

        private static string DecryptApiKey(byte[] encryptedData)
        {
            if (encryptedData.Length < SaltSize + IvSize)
            {
                throw new InvalidDataException("Encrypted API key data is corrupted.");
            }

            var salt = encryptedData.AsSpan(0, SaltSize).ToArray();
            var iv = encryptedData.AsSpan(SaltSize, IvSize).ToArray();
            var cipher = encryptedData.AsSpan(SaltSize + IvSize).ToArray();
            var key = DeriveKey(GetEncryptionPassphrase(), salt);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            byte[] derivedKey = Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return derivedKey;
        }
    }
}