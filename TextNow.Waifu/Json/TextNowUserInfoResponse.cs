using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TextNow.Waifu.Json
{
    public class Sip
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("proxy")]
        public string Proxy { get; set; }

        [JsonProperty("stun")]
        public string Stun { get; set; }

        [JsonProperty("voicemail_url")]
        public string VoicemailUrl { get; set; }
    }

    public class Features
    {
        [JsonProperty("cdma_fallback")]
        public bool CdmaFallback { get; set; }

        [JsonProperty("e911_accepted")]
        public bool E911Accepted { get; set; }
    }

    public class TextNowUserInfoResponse
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public int EmailVerified { get; set; }

        [JsonProperty("first_name")]
        public object FirstName { get; set; }

        [JsonProperty("last_name")]
        public object LastName { get; set; }

        [JsonProperty("captcha_required")]
        public bool CaptchaRequired { get; set; }

        [JsonProperty("last_update")]
        public string LastUpdate { get; set; }

        [JsonProperty("ringtone")]
        public string Ringtone { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("show_text_previews")]
        public bool ShowTextPreviews { get; set; }

        [JsonProperty("forward_messages")]
        public int ForwardMessages { get; set; }

        [JsonProperty("incentivized_share_date_twitter")]
        public string IncentivizedShareDateTwitter { get; set; }

        [JsonProperty("incentivized_share_date_facebook")]
        public string IncentivizedShareDateFacebook { get; set; }

        [JsonProperty("credits")]
        public int Credits { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("purchases_timestamp")]
        public string PurchasesTimestamp { get; set; }

        [JsonProperty("has_password")]
        public bool HasPassword { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("phone_assigned_date")]
        public string PhoneAssignedDate { get; set; }

        [JsonProperty("phone_last_unassigned")]
        public string PhoneLastUnassigned { get; set; }

        [JsonProperty("sip")]
        public Sip Sip { get; set; }

        [JsonProperty("disable_calling")]
        public string DisableCalling { get; set; }

        [JsonProperty("mytempnumber_dnd")]
        public bool MytempnumberDnd { get; set; }

        [JsonProperty("sip_minutes")]
        public int SipMinutes { get; set; }

        [JsonProperty("sip_IP")]
        public string SipIP { get; set; }

        [JsonProperty("mytempnumber_voicemail_upload_url")]
        public string MytempnumberVoicemailUploadUrl { get; set; }

        [JsonProperty("sip_username")]
        public string SipUsername { get; set; }

        [JsonProperty("sip_password")]
        public string SipPassword { get; set; }

        [JsonProperty("mytempnumber_voicemail_v2")]
        public int MytempnumberVoicemailV2 { get; set; }

        [JsonProperty("referring_amount")]
        public int ReferringAmount { get; set; }

        [JsonProperty("referred_amount")]
        public int ReferredAmount { get; set; }

        [JsonProperty("mytempnumber_status")]
        public int MytempnumberStatus { get; set; }

        [JsonProperty("mytempnumber_expiry")]
        public string MytempnumberExpiry { get; set; }

        [JsonProperty("features")]
        public Features Features { get; set; }

        [JsonProperty("forwarding_expiry")]
        public string ForwardingExpiry { get; set; }

        [JsonProperty("forwarding_status")]
        public string ForwardingStatus { get; set; }

        [JsonProperty("premium_calling")]
        public bool PremiumCalling { get; set; }

        [JsonProperty("forwarding_number")]
        public string ForwardingNumber { get; set; }

        [JsonProperty("voicemail")]
        public string Voicemail { get; set; }

        [JsonProperty("voicemail_timestamp")]
        public string VoicemailTimestamp { get; set; }

        [JsonProperty("show_ads")]
        public bool ShowAds { get; set; }

        [JsonProperty("is_persistent")]
        public bool IsPersistent { get; set; }

        [JsonProperty("mytempnumber_free_calling")]
        public int MytempnumberFreeCalling { get; set; }

        [JsonProperty("incentivized_share_date")]
        public string IncentivizedShareDate { get; set; }

        [JsonProperty("append_footer")]
        public int AppendFooter { get; set; }

        [JsonProperty("ads_autorenew")]
        public string AdsAutorenew { get; set; }

        [JsonProperty("voice_autorenew")]
        public string VoiceAutorenew { get; set; }

        [JsonProperty("forward_email")]
        public string ForwardEmail { get; set; }

        [JsonProperty("mytempnumber_voicemail")]
        public bool MytempnumberVoicemail { get; set; }

        [JsonProperty("phone_expiry")]
        public string PhoneExpiry { get; set; }

        [JsonProperty("area_code")]
        public object AreaCode { get; set; }

        [JsonProperty("unlimited_calling")]
        public bool UnlimitedCalling { get; set; }

        [JsonProperty("vm_transcription_enabled")]
        public bool VmTranscriptionEnabled { get; set; }

        [JsonProperty("vm_transcription_user_enabled")]
        public object VmTranscriptionUserEnabled { get; set; }

        [JsonProperty("ad_categories")]
        public IList<object> AdCategories { get; set; }
    }
}
