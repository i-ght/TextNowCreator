using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class TextNowRegisterResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("captcha_link")]
        public string CaptchaLink { get; set; }
    }

    public class TextNowCaptchaRequiredRegisterResponse
    {
        [JsonProperty("result")]
        public TextNowRegisterResult Result { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}
