using System.Collections.Generic;
using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class TextNowPhoneNumberReservationReult
    {

        [JsonProperty("reservation_id")]
        public string ReservationId { get; set; }

        [JsonProperty("phone_numbers")]
        public IList<string> PhoneNumbers { get; set; }
    }

    public class TextNowPhoneNumberReservationResponse
    {

        [JsonProperty("result")]
        public TextNowPhoneNumberReservationReult Result { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}
