using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class CryptoExtensions
    {
        /// <summary>
        /// Mã hóa MD5
        /// </summary>
        public static string ToMD5Hash(this string value)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// Mã hóa string theo giải thuật RSA
        /// </summary>
        public static string ToRSAEncryptString(this string inputString, int dwKeySize, string xmlString)
        {
            // TODO: Add Proper Exception Handlers
            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int keySize = dwKeySize / 8;
            byte[] bytes = Encoding.UTF32.GetBytes(inputString);
            // The hash function in use by the .NET RSACryptoServiceProvider here is SHA1
            // int maxLength = ( keySize ) - 2 - ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
            int maxLength = keySize - 42;
            int dataLength = bytes.Length;
            int iterations = dataLength / maxLength;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i <= iterations; i++)
            {
                byte[] tempBytes = new byte[(dataLength - maxLength * i > maxLength) ? maxLength : dataLength - maxLength * i];
                Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0, tempBytes.Length);
                byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt(tempBytes, true);
                // Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the DecryptString function.
                Array.Reverse(encryptedBytes);
                // Why convert to base 64?
                // Because it is the largest power-of-two base printable using only ASCII characters
                stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Giải mã string theo giải thuật RSA
        /// </summary>
        public static string RSADecryptString(this string inputString, int dwKeySize, string xmlString)
        {
            // TODO: Add Proper Exception Handlers
            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ? (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
            int iterations = inputString.Length / base64BlockSize;
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < iterations; i++)
            {
                byte[] encryptedBytes = Convert.FromBase64String(inputString.Substring(base64BlockSize * i, base64BlockSize));
                // Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the EncryptString function.
                Array.Reverse(encryptedBytes);
                arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(encryptedBytes, true));
            }
            return Encoding.UTF32.GetString(arrayList.ToArray(Type.GetType("System.Byte")) as byte[]);
        }

        /// <summary>
        /// Mã hóa theo giải thuật mã hóa đối xứng EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        /// <param name="vector">Vector khởi tạo của giải thuật mã hóa</param>
        public static string ToAESEncryptString(this string value, string key, string vector)
        {
            return RijindaelCrypto.EncryptString(value, key, vector);
        }

        /// <summary>
        /// Mã hóa theo giải thuật mã hóa đối xứng EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        public static string ToAESEncryptString(this string value, string key)
        {
            return RijindaelCrypto.EncryptString(value, key);
        }

        /// <summary>
        /// Mã hóa theo giải thuật mã hóa đối xứng EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        /// <param name="vector">Vector khởi tạo của giải thuật mã hóa</param>
        public static string ToAESEncryptString(this string value, byte[] key, byte[] vector)
        {
            return RijindaelCrypto.EncryptString(value, key, vector);
        }

        /// <summary>
        /// Mã hóa xâu đã được mã hóa bằng giải thuật EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        /// <param name="vector">Vector khởi tạo của giải thuật mã hóa</param>
        public static string AESDecryptString(this string value, string key, string vector)
        {
            return RijindaelCrypto.DecryptString(value, key, vector);
        }

        /// <summary>
        /// Mã hóa xâu đã được mã hóa bằng giải thuật EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        public static string AESDecryptString(this string value, string key)
        {
            return RijindaelCrypto.DecryptString(value, key);
        }

        /// <summary>
        /// Mã hóa xâu đã được mã hóa bằng giải thuật EAS
        /// </summary>
        /// <param name="key">Khóa mã hóa - giải mã</param>
        /// <param name="vector">Vector khởi tạo của giải thuật mã hóa</param>
        public static string AESDecryptString(this string value, byte[] key, byte[] vector)
        {
            return RijindaelCrypto.DecryptString(value, key, vector);
        }
    }
}
