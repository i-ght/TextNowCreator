using System.Collections.Generic;
using System.Net;
using DankWaifu.Collections;
using DankWaifu.Net;

namespace TextNow.Waifu
{
    public class TextNowApiRequest
    {
        public TextNowApiRequest(HttpMethod httpMethod, string endpoint)
        {
            HttpMethod = httpMethod;
            Endpoint = endpoint;
            QueryParams = new Dictionary<string, string>();
            PostParams = new Dictionary<string, string>();
        }

        public HttpMethod HttpMethod { get; }
        public string Endpoint { get; }
        public Dictionary<string, string> QueryParams { get; set; }
        public Dictionary<string, string> PostParams { get; set; }

        public HttpReq ToHttpReq()
        {
            var signature = TextNowCrypto.CalculateSignature(HttpMethod, Endpoint, QueryParams, PostParams);
            QueryParams.Add("signature", signature);

            var contentType = string.Empty;
            var contentBody = string.Empty;
            if (PostParams.Count > 0)
            {
                contentType = "application/x-www-form-urlencoded";
                contentBody = PostParams.ToUrlEncodedQueryString();
            }

            var request = new HttpReq(HttpMethod, $"https://api.textnow.me/api2.0/{Endpoint}?{QueryParams.ToUrlEncodedQueryString()}")
            {
                AcceptEncoding =  DecompressionMethods.GZip,
                ContentType = contentType,
                ContentBody = contentBody
            };

            return request;
        }
    }
}
