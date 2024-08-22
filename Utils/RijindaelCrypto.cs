using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class RijindaelCrypto
    {
        /// <summary>
        /// Khởi tạo bộ key
        /// </summary>
        public static string GenerateRandomKey()
        {
            byte[] key = new byte[32];
            for (int i = 0; i < 32; i++)
                key[i] = (byte)GenerateRandomNumber(0, 255);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Khởi tạo vector
        /// </summary>
        public static string GenerateRandomIV()
        {
            byte[] key = new byte[16];
            for (int i = 0; i < 16; i++)
                key[i] = (byte)GenerateRandomNumber(0, 255);
            return Convert.ToBase64String(key);
        }

        private static byte[] _iv = new byte[16] { 78, 86, 70, 196, 179, 86, 46, 68, 131, 119, 217, 14, 37, 149, 254, 250 };

        /// <summary>
        /// Gets or sets the initialization vector (System.Security.Cryptography.SymmetricAlgorithm.IV) for the symmetric algorithm.
        /// </summary>
        public static byte[] IV
        {
            get
            {
                if (_iv == null)
                {
                    _iv = new byte[16];
                    for (int i = 0; i < 16; i++)
                        _iv[i] = (byte)GenerateRandomNumber(0, 255);
                }
                return _iv;
            }
            set
            {
                _iv = value;
            }
        }

        public static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            try
            {
                // Create an RijndaelManaged object 
                // with the specified key and IV. 
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(key, iv);
                    // Create the streams used for encryption. 
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                //Write all data to the stream.
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string EncryptString(string plainText, string key)
        {
            return EncryptString(plainText, Convert.FromBase64String(key), IV);
        }

        public static string EncryptString(string plainText, string key, string iv)
        {
            return EncryptString(plainText, Convert.FromBase64String(key), Convert.FromBase64String(iv));
        }

        public static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            try
            {
                // Create an RijndaelManaged object 
                // with the specified key and IV. 
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(key, iv);

                    // Create the streams used for decryption. 
                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream 
                                // and place them in a string.
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string DecryptString(string cipherText, string key)
        {
            return DecryptString(cipherText, Convert.FromBase64String(key), IV);
        }

        public static string DecryptString(string cipherText, string key, string iv)
        {
            return DecryptString(cipherText, Convert.FromBase64String(key), Convert.FromBase64String(iv));
        }

        /// <summary>
        /// Khởi tạo bộ số ngẫu nhiên
        /// </summary>
        /// <returns></returns>
        private static int GenerateRandomNumber(int minValue, int maxValue)
        {
            // We will make up an integer seed from 4 bytes of this array.
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
                // Convert four random bytes into a positive integer value.
                int seed = ((randomBytes[0] & 0x7f) << 24) | (randomBytes[1] << 16) | (randomBytes[2] << 8) | (randomBytes[3]);

                // Now, this looks more like real randomization.
                Random random = new Random(seed);
                // Calculate a random number.
                return random.Next(minValue, maxValue + 1);
            }
        }
    }
}
