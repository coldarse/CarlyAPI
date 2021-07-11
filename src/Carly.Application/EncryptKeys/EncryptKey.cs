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
                rm.Key = Encoding.ASCII.GetBytes(CryptKey);
                byte[] inputByteArray = Encoding.ASCII.GetBytes(value);
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
                rm.Key = Encoding.ASCII.GetBytes(CryptKey);
                byte[] inputByteArray = Encoding.ASCII.GetBytes(value.Replace(" ", "+"));
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, rm.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Encoding.ASCII.GetString(ms.ToArray());
            }
            catch
            {
                return "";
            }
        }
    }
}
