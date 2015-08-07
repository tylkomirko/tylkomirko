using System;
using System.Linq;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace WykopSDK.Utils
{
    public static class Cryptography
    {
        public static string EncodeBase64(string input)
        {
            if (input == null) return "";

            var plainTextBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeBase64(string input)
        {
            if (input == null) return "";

            var base64EncodedBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Count());
        }

        public static string EncodeMD5(string input)
        {
            if (input == null) return "";

            var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }
    }
}
