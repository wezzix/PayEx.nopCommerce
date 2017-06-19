using System;
using System.Security.Cryptography;
using System.Text;

namespace SD.Payex2.Crypto
{
    /// <summary>
    /// MD5 hashes input and returns 32 hexadecimal characters.
    /// </summary>
    internal static class MD5Hash
    {
        public static bool Hash(string data, out string hash)
        {
            try
            {
                var hasher = new MD5CryptoServiceProvider();
                hasher.Initialize();
                //hasher
                byte[] bytes;
                bytes = hasher.ComputeHash(Encoding.Default.GetBytes(data));

                var sb = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("X2"));

                hash = sb.ToString();
                return true;
            }
            catch (Exception)
            {
                hash = null;
                return false;
            }
        }
    }
}