using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class BonusInfo
    {

        [JsonProperty("AdvertisingIdInfo")]
        public string AdvertisingIdInfo { get; set; }

        [JsonProperty("TelephonyManagerSimSerialNumber")]
        public object TelephonyManagerSimSerialNumber { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("BatteryLevel")]
        public string BatteryLevel { get; set; }

        [JsonProperty("IsUserAMonkey")]
        public string IsUserAMonkey { get; set; }

        [JsonProperty("ScreenWidth")]
        public string ScreenWidth { get; set; }

        [JsonProperty("WiFiMACAddress")]
        public string WiFiMacAddress { get; set; }

        [JsonProperty("BluetoothMACAddress")]
        public string BluetoothMacAddress { get; set; }

        [JsonProperty("TelephonyManagerDeviceId")]
        public string TelephonyManagerDeviceId { get; set; }

        [JsonProperty("Uptime (dd hh mm)")]
        public string UptimeDdHhMm { get; set; }

        [JsonProperty("AndroidId")]
        public string AndroidId { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("ScreenHeight")]
        public string ScreenHeight { get; set; }

        [JsonProperty("TimeZone")]
        public string TimeZone { get; set; }
    }

    public class TextNowRegisterInfo
    {

        [JsonProperty("bonus_info")]
        public BonusInfo BonusInfo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
