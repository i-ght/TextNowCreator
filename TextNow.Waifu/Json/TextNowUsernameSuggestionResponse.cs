using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class TextNowUsernameSuggestionResponse
    {
        [JsonProperty("username")]
        public string Username { get; set; }
    }

}
