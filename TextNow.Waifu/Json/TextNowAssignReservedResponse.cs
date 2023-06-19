using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class TextNowAssignReservedResult
    {
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
    }

    public class TextNowAssignReservedResponse
    {
        [JsonProperty("result")]
        public TextNowAssignReservedResult Result { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}
