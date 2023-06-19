using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class TextNowStatus
    {
        [JsonProperty("user_timestamp")]
        public DateTime UserTimestamp { get; set; }

        [JsonProperty("settings_version")]
        public string SettingsVersion { get; set; }

        [JsonProperty("features_version")]
        public string FeaturesVersion { get; set; }

        [JsonProperty("latest_message_id")]
        public long LatestMessageId { get; set; }

        [JsonProperty("num_devices")]
        public int NumDevices { get; set; }
    }

    public class TextNowMessage
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("contact_value")]
        public string ContactValue { get; set; }

        [JsonProperty("e164_contact_value")]
        public object E164ContactValue { get; set; }

        [JsonProperty("contact_type")]
        public int ContactType { get; set; }

        [JsonProperty("contact_name")]
        public string ContactName { get; set; }

        [JsonProperty("message_direction")]
        public int MessageDirection { get; set; }

        [JsonProperty("message_type")]
        public int MessageType { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public class TextNowMessagesResponse
    {
        [JsonProperty("status")]
        public TextNowStatus Status { get; set; }

        [JsonProperty("messages")]
        public IList<TextNowMessage> Messages { get; set; }
    }
}
