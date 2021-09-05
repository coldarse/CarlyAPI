using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Carly.EncryptKeys
{
    public static class EncryptKey
    {
        private static string CryptKey = "mYq3t6w9z$C&F)J@";

        public static string Encrypt(string value)
        {
            RijndaelManaged rm = new RijndaelManaged();
            try
            {
                rm.Padding = PaddingMode.PKCS7;
                rm.BlockSize = 128;
                rm.KeySize = 128;
                rm.Mode = CipherMode.ECB;
                rm.Key = Encoding.UTF8.GetBytes(CryptKey);
                byte[] inputByteArray = Encoding.UTF8.GetBytes(value);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, rm.CreateEncryptor(rm.Key, rm.IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return "";
            }
        }

        public static string Decrypt(string value)
        {
            RijndaelManaged rm = new RijndaelManaged();
            try
            {
                rm.Padding = PaddingMode.PKCS7;
                rm.BlockSize = 128;
                rm.KeySize = 128;
                rm.Mode = CipherMode.ECB;
                rm.Key = Encoding.UTF8.GetBytes(CryptKey);
                byte[] inputByteArray = Convert.FromBase64String(value.Replace(" ", "+"));
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, rm.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}
