using System.Net;
using System.Threading.Tasks;
using DankWaifu.Net;

namespace TextNow.Waifu
{
    public abstract class TextNowClientBase
    {
        private readonly HttpWaifu _client;
        private readonly string _userAgent;

        /// <summary>
        /// <see cref="System.String"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="device"></param>
        protected TextNowClientBase(
            HttpWaifu client,
            TextNowAndroidDevice device)
        {
            _client = client;
            _userAgent = $"TextNow {TextNowConstants.Version} ({device.Model}; Android OS {device.OsVersion}; en_US)";
        }

        public WebProxy Proxy
        {
            get => _client.Config.Proxy;
            set
            {
                if (value == null)
                    return;
                _client.Config.Proxy = value;
            }
        }

        protected async Task<HttpResp> SendHttpRequest(HttpReq httpReq)
        {
            return await _client.SendRequestAsync(httpReq)
                .ConfigureAwait(false);
        }

        protected async Task<HttpResp> SendApiRequest(TextNowApiRequest apiRequest)
        {
            var httpReq = apiRequest.ToHttpReq();
            httpReq.OverrideUserAgent = _userAgent;
            var response = await _client.SendRequestAsync(httpReq)
                .ConfigureAwait(false);
            return response;
        }

        protected static TextNowException CreateInvalidStatusCodeEx(string endpoint, HttpStatusCode statusCode)
        {
            return new TextNowException(
                $"The request made to api.textnow.me/api2.0/{endpoint} returned invalid response status code. ({statusCode})"
            );
        }

        protected static TextNowException CreateInvalidBodyEx(string endpoint, HttpStatusCode statusCode)
        {
            return new TextNowException(
                $"The request made to api.textnow.me/api2.0/{endpoint} returned invalid response body. ({statusCode})"
            );
        }

        protected static TextNowException CreateInvalidJsonResultEx(string endpoint, HttpStatusCode statusCode)
        {
            return new TextNowException(
                $"The request made to api.textnow.me/api2.0/{endpoint} returned a null result json object. ({statusCode})"
            );
        }

        protected static TextNowException CreateInvalidErrorCodeEx(string endpoint, string errorCode, HttpStatusCode statusCode)
        {
            return new TextNowException(
                $"The request made to api.textnow.me/api2.0/{endpoint} returned an error code {errorCode}. ({statusCode})"
            );
        }

        protected static TextNowException CreateTextNowApiEx(string endpoint, string message, HttpStatusCode statusCode)
        {
            return new TextNowException(
                $"The request made to api.textnow.me/api2.0/{endpoint} {message}. ({statusCode})"
            );
        }

    }
}
