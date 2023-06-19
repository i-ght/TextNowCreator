using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DankWaifu.Collections;
using DankWaifu.Net;

namespace TextNow.Waifu
{
    public static class TextNowCrypto
    {
        private const string Salt = "f8ab2ceca9163724b6d126aea9620339";

        public static string CalculateSignature(
            HttpMethod httpMethod,
            string endpoint,
            Dictionary<string, string> queryParams,
            string jsonHttpCotent = "")
        {
            var sb = new StringBuilder(
                $"{Salt}{httpMethod}{endpoint}?{queryParams.ToUrlEncodedQueryString()}"
            );

            if (!string.IsNullOrWhiteSpace(jsonHttpCotent))
                sb.Append(jsonHttpCotent);
            var input = sb.ToString();
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(inputBytes);
                var hex = BytesToHex(hash);
                return hex;
            }
        }

        public static string CalculateSignature(
            HttpMethod httpMethod,
            string endpoint,
            Dictionary<string, string> queryParams,
            Dictionary<string, string> postParams)
        {
            var jsonContentBody = string.Empty;
            if (postParams.ContainsKey("json"))
                jsonContentBody = postParams["json"];

            return CalculateSignature(httpMethod, endpoint, queryParams, jsonContentBody);
        }

        private static string BytesToHex(IEnumerable<byte> input)
        {
            var sb = new StringBuilder();
            foreach (var @byte in input)
               sb.Append(@byte.ToString("x2"));
            var ret = sb.ToString();
            sb.Clear();
            return ret;
        }
    }
}
