using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace KBNovaCMS.Common.Security.EncodeDecode
{
    public static class EncodeDecode
    {
        private static readonly Regex SpecialCharacterRegex = new Regex(@"[\[\]<>=()|;{}+]", RegexOptions.Compiled);
        private static readonly Regex PercentEncodedRegex = new Regex("%([0-9A-Fa-f]{2})", RegexOptions.Compiled);


        public static string EncodeBase64(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return string.Empty;

            try
            {
                var valueBytes = Encoding.UTF8.GetBytes(plaintext);
                return Convert.ToBase64String(valueBytes);
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string DecodeBase64(string encodedText)
        {
            if (string.IsNullOrWhiteSpace(encodedText))
                return string.Empty;

            try
            {
                var valueBytes = Convert.FromBase64String(encodedText);
                return Encoding.UTF8.GetString(valueBytes);
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string EncodeBase64_Custom(this Encoding encoding, string plaintext, bool isUrl = false)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return string.Empty;

            try
            {
                var textAsBytes = encoding.GetBytes(plaintext);
                var base64String = Convert.ToBase64String(textAsBytes);

                return isUrl ? EncodeSpecialCharacters(base64String) : base64String;
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string DecodeBase64_Custom(this Encoding encoding, string encodedText, bool isUrl = false)
        {
            if (string.IsNullOrWhiteSpace(encodedText))
                return string.Empty;

            try
            {
                var textToDecode = isUrl ? DecodeSpecialCharacters(encodedText) : encodedText;
                var textAsBytes = Convert.FromBase64String(textToDecode);
                return encoding.GetString(textAsBytes);
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string ToHexString(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return string.Empty;

            try
            {
                var bytes = Encoding.Unicode.GetBytes(plaintext);
                return string.Concat(bytes.Select(b => b.ToString("X2")));
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string FromHexString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
                return string.Empty;

            try
            {
                var bytes = Enumerable.Range(0, hexString.Length / 2)
                                      .Select(x => Convert.ToByte(hexString.Substring(x * 2, 2), 16))
                                      .ToArray();
                return Encoding.Unicode.GetString(bytes);
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string ToHexUTF8String(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return string.Empty;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(plaintext);
                return string.Concat(bytes.Select(b => b.ToString("X2")));
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string FromHexUTF8String(string hexUTF8String)
        {
            if (string.IsNullOrWhiteSpace(hexUTF8String))
                return string.Empty;

            try
            {
                var bytes = Enumerable.Range(0, hexUTF8String.Length / 2)
                                      .Select(x => Convert.ToByte(hexUTF8String.Substring(x * 2, 2), 16))
                                      .ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                throw; // Consider logging the exception or handling it accordingly
            }
        }

        public static string EncodeSpecialCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Replace each special character with its percent-encoded equivalent
            string encoded = SpecialCharacterRegex.Replace(text, match =>
            {
                // Percent-encode each special character
                return HttpUtility.UrlEncode(match.Value);
            });

            return encoded;
        }

        public static string DecodeSpecialCharacters(string encodedText)
        {
            if (string.IsNullOrWhiteSpace(encodedText))
                return string.Empty;

            try
            {
                // Decode percent-encoded characters
                return PercentEncodedRegex.Replace(encodedText, match =>
                {
                    // Convert hex value to integer and then to character
                    var hexValue = match.Groups[1].Value;
                    var decodedChar = (char)Convert.ToInt32(hexValue, 16);
                    return decodedChar.ToString();
                });
            }
            catch (Exception ex)
            {
                // Log exception or handle as appropriate
                Console.WriteLine($"Error decoding special characters: {ex.Message}");
                throw;
            }
        }
    }
}
namespace KBNovaCMS.Common.Security.EncryptDecrypt
{
    public static class StringCipher
    {
        //private static readonly byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("AES_KEY"));
        //private static readonly byte[] IV = Convert.FromBase64String(Environment.GetEnvironmentVariable("AES_IV"));

        private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // Use environment variables or a secure store
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("0123456789ABCDEF");

        private static Aes CreateAesCipher(byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;

            using (var aes = CreateAesCipher(Key, IV))
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Flush();
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText)) return string.Empty;

            var cipherBytes = Convert.FromBase64String(encryptedText);

            using (var aes = CreateAesCipher(Key, IV))
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherBytes))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
    public static class EncryptDecrypt
    {
        private static readonly byte[] byteArray_Key = Encoding.UTF8.GetBytes("ha29rdik101a99m4");
        private static readonly byte[] byteArray_IV = Encoding.UTF8.GetBytes("ha29rdik101a99m4");

        public static string Key = "4090909090909020";
        public static string IV = "4090909090909020";
        public static string Key_1 = "4080808080808020";
        public static string IV_1 = "4080808080808020";

        private static Aes CreateAesInstance(byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        #region Encrypt - Decrypt String AES
        public static string EncryptStringAES(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;

            try
            {
                using (var aes = CreateAesInstance(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(IV)))
                {
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                        sw.Close();
                        return Convert.ToBase64String(ms.ToArray());//Convert Base-64 String From byte[]
                    }
                }
            }
            catch
            {
                throw; // Consider logging the exception
            }
        }

        public static string DecryptStringAES(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText)) return string.Empty;

            try
            {
                var cipherBytes = Convert.FromBase64String(encryptedText);

                using (var aes = CreateAesInstance(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(IV)))
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipherBytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                throw; // Consider logging the exception
            }
        }
        #endregion

        #region Encrypt - Decrypt To Bytes Using CBC
        public static byte[] EncryptToBytesUsingCBC(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return new byte[] { };

            try
            {
                using (var aes = CreateAesInstance(byteArray_Key, byteArray_IV))
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                        sw.Close();
                        return ms.ToArray();
                    }
                }
            }
            catch
            {
                throw; // Consider logging the exception
            }
        }

        public static string DecryptToBytesUsingCBC(byte[] encryptedArray)
        {
            if (encryptedArray == null || encryptedArray.Length == 0) return string.Empty;

            try
            {
                using (var aes = CreateAesInstance(byteArray_Key, byteArray_IV))
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(encryptedArray))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                throw; // Consider logging the exception
            }
        }
        #endregion

        #region Encrypt - Decrypt Using CBC
        public static string EncryptUsingCBC(string plainText)
        {
            return Convert.ToBase64String(EncryptToBytesUsingCBC(plainText));
        }

        public static string DecryptUsingCBC(string encryptedText)
        {
            return DecryptToBytesUsingCBC(Convert.FromBase64String(encryptedText));
        }
        #endregion

        #region Encrypt - Decrypt
        public static string Encrypt(string plainText, bool isHtmlEncode = false)
        {
            return EncryptUsingCBC(plainText);
        }

        public static string Decrypt(string encryptedText, bool isHtmlEncode = false)
        {
            return DecryptUsingCBC(encryptedText);
        }
        #endregion

        #region Encrypt - Decrypt #Front#
        public static string FrontEncrypt(string plainText)
        {
            return EncryptStringAES(plainText);
        }
        public static string FrontEncryptEncode(string plainText)
        {
            return EncodeDecode.EncodeDecode.EncodeSpecialCharacters(EncodeDecode.EncodeDecode.EncodeBase64(FrontEncrypt(plainText)));
            //return EncodeDecode.EncodeDecode.EncodeSpecialCharacters(FrontEncrypt(plainText));
        }
        public static string FrontDecrypt(string encryptedText)
        {
            return DecryptStringAES(encryptedText);
        }
        public static string FrontDecryptDecode(string plainText)
        {
            return FrontDecrypt(EncodeDecode.EncodeDecode.DecodeBase64(EncodeDecode.EncodeDecode.DecodeSpecialCharacters(plainText)));
            //return FrontDecrypt(EncodeDecode.EncodeDecode.DecodeSpecialCharacters(plainText));
        }
        #endregion

        #region Encrypt - Decrypt #JWT#
        public static string JWTEncrypt(string plainText)
        {
            return StringCipher.Encrypt(plainText); // Ensure StringCipher is properly implemented
        }

        public static string JWTDecrypt(string encryptedText)
        {
            return StringCipher.Decrypt(encryptedText); // Ensure StringCipher is properly implemented
        }
        #endregion
    }
}
namespace KBNovaCMS.Common.Security.Hash
{
    public static class Hash
    {
        private static string ComputeHash(HashAlgorithm algorithm, string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using (algorithm)
            {
                byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
        public static string GetMD5Hash(string input) => ComputeHash(MD5.Create(), input);
        public static string GetSHA1Hash(string input) => ComputeHash(SHA1.Create(), input);
        public static string GetSHA256Hash(string input) => ComputeHash(SHA256.Create(), input);
        public static string GetSHA384Hash(string input) => ComputeHash(SHA384.Create(), input);
        public static string GetSHA512Hash(string input) => ComputeHash(SHA512.Create(), input);

        //HMAC combines a hash function with a secret key to provide message integrity and authenticity. It is used in various security protocols.
        public static string GetHMACSHA256Hash(string input, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        // Method to compute SHA256 hash of an object
        public static string GetHashOfObject<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            try
            {
                // Serialize the object to JSON
                string jsonString = JsonSerializer.Serialize(obj);

                // Convert JSON string to byte array
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

                // Compute SHA256 hash of the byte array
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(bytes);
                    return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception or adding more context if needed
                throw new InvalidOperationException("Error computing hash of the object.", ex);
            }
        }
        public static string GetMD5Hash(IFormFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var md5 = MD5.Create())
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception or adding more context if needed
                throw new InvalidOperationException("Error computing MD5 hash.", ex);
            }
        }
    }
}