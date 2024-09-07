/*using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Duha.SIMS.BAL.Interface
{
    public class PasswordEncryptHelper : IPasswordEncryptHelper
    {
        private readonly byte[] _key;

        public PasswordEncryptHelper()
        {
            string keyString = "#$%wellandgoodduha-softwaGSVDAhgde";
            _key = Encoding.UTF8.GetBytes(keyString);
            if (_key.Length != 32)
            {
                Array.Resize(ref _key, 32); // Ensure the key is exactly 32 bytes
            }
        }

        public async Task<string> ProtectAsync<T>(T data, string encryptionKey = "")
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            var encryptedData = EncryptString(jsonData, _key);
            return await Task.FromResult(encryptedData);
        }

        public async Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "")
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            var jsonData = DecryptString(encryptedData, _key);
            var data = System.Text.Json.JsonSerializer.Deserialize<T>(jsonData);
            return await Task.FromResult(data);
        }

        private string EncryptString(string plainText, byte[] key)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                var iv = DeriveIV(plainText, aesAlg.BlockSize / 8);
                aesAlg.IV = iv;

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private string DecryptString(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aesAlg = Aes.Create())
            {
                var iv = DeriveIV(cipherText, aesAlg.BlockSize / 8);
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (var msDecrypt = new MemoryStream(fullCipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        private byte[] DeriveIV(string input, int length)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                Array.Resize(ref hash, length);
                return hash;
            }
        }
    }
}*/

/*using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Duha.SIMS.BAL.Interface
{
    public class PasswordEncryptHelper : IPasswordEncryptHelper
    {
        private readonly byte[] _key;

        public PasswordEncryptHelper()
        {
            string keyString = "#$%wellandgoodduha-softwaGSVDAhgde";
            _key = Encoding.UTF8.GetBytes(keyString);
            if (_key.Length != 32)
            {
                Array.Resize(ref _key, 32); // Ensure the key is exactly 32 bytes
            }
        }

        public async Task<string> ProtectAsync<T>(T data, string encryptionKey = "")
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            var encryptedData = EncryptString(jsonData, _key);
            return await Task.FromResult(encryptedData);
        }

        public async Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "")
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            var jsonData = DecryptString(encryptedData, _key);
            var data = System.Text.Json.JsonSerializer.Deserialize<T>(jsonData);
            return await Task.FromResult(data);
        }

        private string EncryptString(string plainText, byte[] key)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();
                var iv = aesAlg.IV;

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    // Write IV to the beginning of the stream
                    msEncrypt.Write(iv, 0, iv.Length);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private string DecryptString(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aesAlg = Aes.Create())
            {
                var iv = new byte[aesAlg.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                // Read IV from the beginning of the cipher
                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}
*/


using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Duha.SIMS.BAL.Interface
{
    public class PasswordEncryptHelper : IPasswordEncryptHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _fixedIV = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // 16 bytes IV

        public PasswordEncryptHelper()
        {
            string keyString = "#$%wellandgoodduha-softwaGSVDAhgde";
            _key = Encoding.UTF8.GetBytes(keyString);
            if (_key.Length != 32)
            {
                Array.Resize(ref _key, 32); // Ensure the key is exactly 32 bytes
            }
        }

        public async Task<string> ProtectAsync<T>(T data, string encryptionKey = "")
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            var encryptedData = EncryptString(jsonData, _key);
            return await Task.FromResult(encryptedData);
        }

        public async Task<T> UnprotectAsync<T>(string encryptedData, string decryptionKey = "")
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            var jsonData = DecryptString(encryptedData, _key);
            var data = System.Text.Json.JsonSerializer.Deserialize<T>(jsonData);
            return await Task.FromResult(data);
        }

        private string EncryptString(string plainText, byte[] key)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _fixedIV; // Use fixed IV

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private string DecryptString(string cipherText, byte[] key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _fixedIV; // Use fixed IV

                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (var msDecrypt = new MemoryStream(fullCipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}



