using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DankWaifu.Collections;
using DankWaifu.Net;
using DankWaifu.Sys;
using Newtonsoft.Json;
using TextNow.Waifu.Json;

namespace TextNow.Waifu
{
    public class TextNowClientLoggedOut : TextNowClientBase
    {
        private static readonly Regex CaptchaTokenRegex;

        static TextNowClientLoggedOut()
        {
            CaptchaTokenRegex = new Regex("token=(.*?)&", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public TextNowClientLoggedOut(
            HttpWaifu client,
            TextNowAndroidDevice device) : base(client, device) { }

        public async Task<bool> RetrieveEmailAvailability(string email)
        {
            const HttpMethod httpMethod = HttpMethod.HEAD;
            var endpoint = $"emails/{HttpHelpers.UrlEncode(email)}";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return true;

            if (response.StatusCode == HttpStatusCode.OK)
                return false;

            throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);
        }

        public async Task<bool> RetrieveUsernameAvailability(string username)
        {
            const HttpMethod httpMethod = HttpMethod.HEAD;
            var endpoint = $"users/{HttpHelpers.UrlEncode(username)}";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return true;

            if (response.StatusCode == HttpStatusCode.OK)
                return false;

            throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);
        }

        public async Task<string> RetrieveUsernameSuggestion(string username)
        {
            const HttpMethod httpMethod = HttpMethod.POST;
            const string endpoint = "users/suggestions";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };
            var postParams = new Dictionary<string, string>()
            {
                ["json"] = $"{{\"base_names\":[\"{username}\"]}}"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                PostParams = postParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);

            if (!response.ContentBody.Contains("username"))
                throw CreateInvalidBodyEx(endpoint, response.StatusCode);

            var json = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowUsernameSuggestionResponse>(
                    response.ContentBody
                )
            );
            if (string.IsNullOrWhiteSpace(json.Username))
            {
                throw CreateTextNowApiEx(
                    endpoint,
                    "did not return a username",
                    response.StatusCode
                );
            }

            return json.Username;
        }

        //TODO: Check response if not captchaed
        public async Task<
            TextNowCaptchaRequiredRegisterResponse
        > CreateAccount(
            string username,
            TextNowRegisterInfo
            registerInfo)
        {
            const HttpMethod httpMethod = HttpMethod.PUT;
            var endpoint = $"users/{username}";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };

            var json = await Task.Run(
                () => JsonConvert.SerializeObject(
                    registerInfo,
                    Formatting.None
                )
            ).ConfigureAwait(false);

            json = json.Replace("America/", @"America\/");

            var postParams = new Dictionary<string, string>()
            {
                ["json"] = json
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                PostParams = postParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            string contentBody;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                contentBody = $"{{\"result\":{response.ContentBody}}}";
            }
            else
            {
                contentBody = response.ContentBody;
            }

            if (response.StatusCode != HttpStatusCode.Accepted &&
                response.StatusCode != HttpStatusCode.OK)
            {
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);
            }

            var jsonResponse = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowCaptchaRequiredRegisterResponse>(
                    contentBody
                )
            ).ConfigureAwait(false);

            if (jsonResponse.Result == null)
                throw CreateInvalidJsonResultEx(endpoint, response.StatusCode);

            if (string.IsNullOrWhiteSpace(jsonResponse.Result.Id))
            {
                throw CreateTextNowApiEx(
                    endpoint,
                    "did not return a client id",
                    response.StatusCode
                );
            }

            return jsonResponse;
        }

        public async Task<bool> RetrieveValidationOfCaptchaSolution(string captchaLink, string solution, TextNowAndroidDevice device,
            WebProxy overrideProxy = null)
        {
            if (!CaptchaTokenRegex.TryGetGroup(captchaLink, out var token))
                throw new InvalidOperationException("Failed to parse token from textnow captcha url");

            token = Uri.UnescapeDataString(token);

            var postParams = new Dictionary<string, string>
            {
                ["json"] = $"{{\"u\":\"{token}\",\"c\":\"{solution}\"}}"
            };


            const string requestUrl = "https://www.textnow.com/api/identity/validate";
            var request = new HttpReq(HttpMethod.POST, requestUrl)
            {
                Accept = "*/*",
                AcceptEncoding = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Origin = "https://www.textnow.com",
                Referer = captchaLink,
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                OverrideUserAgent = $"Mozilla/5.0 (Linux; Android {device.OsVersion}; {device.Model} Build/{device.Build}) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36",
                AdditionalHeaders = new WebHeaderCollection
                {
                    ["X-Requested-With"] = "XMLHttpRequest",
                    ["Accept-Language"] = "en-US"
                },
                ContentBody = postParams.ToUrlEncodedQueryString(),
                OverrideProxy = overrideProxy
            };

            var response = await SendHttpRequest(request)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
#if DEBUG
                using (var sw = new StreamWriter("debug.txt", true))
                {
                    await sw.WriteLineAsync($"Status code: {response.StatusCode}")
                        .ConfigureAwait(false);
                    await sw.WriteLineAsync($"Content body: {response.ContentBody}")
                        .ConfigureAwait(false);
                }
#endif
                return false;
            }

            if (!response.ContentBody.Contains("\"error_code\":null"))
            {
#if DEBUG
                using (var sw = new StreamWriter("debug.txt", true))
                {
                    await sw.WriteLineAsync($"Status code: {response.StatusCode}")
                        .ConfigureAwait(false);
                    await sw.WriteLineAsync($"Content body: {response.ContentBody}")
                        .ConfigureAwait(false);
                }
#endif
                return false;
            }

            return true;
        }
    }
}
