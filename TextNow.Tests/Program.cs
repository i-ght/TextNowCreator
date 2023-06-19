using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DankWaifu.Android;
using DankWaifu.Celly;
using DankWaifu.Net;
using Newtonsoft.Json;
using TextNow.Waifu;
using TextNow.Waifu.Json;

namespace TextNow.Tests
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            TestParse();
            TestPutRequest();
            TestGetRequest();
            TestHeadRequest();
            TestPostRequest();
            TestTextNowRegisterInfoSerialization();
            TestImeid();
            TestUnescape();
            TestGetRequest2();
        }

        private static void TestUnescape()
        {
            var escapeMe =
                "https:\\/\\/www.textnow.com\\/recaptcha?token=MTU1NjdhNTYwNTZjNTA0M2QwZTZiNmRjOTU0ZDY3ZWOM9xdysVZpkHgV0fGcfqlf%2B6MRMWQQ7xpZI%2FtWYVbzLA%3D%3D\\u0026client_type=TN_ANDROID";
            var unescaped = Regex.Unescape(escapeMe);
        }

        private static void TestGetRequest()
        {
            var method = HttpMethod.GET;
            var endpoint = "users/username/54261971648605";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };

            var signature = TextNowCrypto.CalculateSignature(method, endpoint, queryParams);
            Debug.Assert(signature == "c1105ecaea58bf6251822f7c7e717424");
        }

        private static void TestGetRequest2()
        {
            var method = HttpMethod.GET;
            var endpoint = "users/hjhfttfrr/messages";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = "1495a4c77c8f63797bc40ec35707860742d44708077e35ef8ae3681896921977",
                ["start_message_id"] = "1",
                ["page_size"] = "30",
                ["direction"] = "future",
                ["get_all"] = "1"
            };
            var signature = TextNowCrypto.CalculateSignature(method, endpoint, queryParams);
            Debug.Assert(signature == "4ba8ad46cedd5af60d272bca9d8424df");
        }

        private static void TestHeadRequest()
        {
            var method = HttpMethod.HEAD;
            var endpoint = "emails/hjhfttfrr%40aol.com";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };
            var signature = TextNowCrypto.CalculateSignature(method, endpoint, queryParams);
            Debug.Assert(signature == "cd4f405537ddbced3abdd2dd3cf3f50f");
        }

        public static void TestPostRequest()
        {
            var method = HttpMethod.POST;
            var endpoint = "sessions";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };
            var jsonContentBody =
                "{\"app_version\":\"5.20.0\",\"esn\":\"52813730920744\",\"username\":\"ggdcc\",\"password\":\"fgvcc\",\"os_version\":\"19\"}";

            var signature = TextNowCrypto.CalculateSignature(method, endpoint, queryParams, jsonContentBody);
            Debug.Assert(signature == "29a49b7790ca049b8408e57581710e09");
        }

        private static void TestPutRequest()
        {
            var method = HttpMethod.PUT;
            var endpoint = "users/hjhfttfrr";
            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID"
            };
            var jsonContentBody =
                "{\"bonus_info\":{\"AdvertisingIdInfo\":\"c58c0a53-2329-49cd-a78e-0740e4fa1a8a\",\"TelephonyManagerSimSerialNumber\":null,\"Country\":\"US\",\"BatteryLevel\":\"0.54\",\"IsUserAMonkey\":\"false\",\"ScreenWidth\":\"540\",\"WiFiMACAddress\":\"FC:B4:D2:FF:27:EA\",\"BluetoothMACAddress\":\"DC:EA:BE:DE:AA:21\",\"TelephonyManagerDeviceId\":\"10730012100495\",\"Uptime (dd hh mm)\":\"0 7 45\",\"AndroidId\":\"35701f4743d3eef8\",\"Language\":\"en\",\"ScreenHeight\":\"960\",\"TimeZone\":\"America\\/Denver\"},\"email\":\"hjhfttfrr@aol.com\",\"password\":\"fhdfhugjgf\"}";

            var signature = TextNowCrypto.CalculateSignature(method, endpoint, queryParams, jsonContentBody);

        }

        private static void TestImeid()
        {
            var imeid = CellyHelpers.RandomImei().Substring(0, 14);
            Debug.Assert(imeid.Length == "10730012100495".Length);

            var iccid = CellyHelpers.RandomIccid(CellyHelpers.RandomCellyCarrierInfo());
            Debug.Assert(CellyHelpers.PassesTheLuhnCheck(iccid));
        }

        private static void TestParse()
        {
            var acctInfo =
                "1d3c44141df57bb63ade186e9b8805b7f4bd4790609ea854ced653978343e710:2014734286:demidenkoduncansmith@hotmail.com:demidenkoduncansmith:13dkillgh2:Samsung|GT-I8190|4.4.4|480|800|KTU84P|98f52094-9bd2-42f2-9cd0-f1f69cade086|76;45;13;6B;12;FF|36;67;C0;B4;79;3D|44264905720234|ee013f8ea74d1164|America/New_York|89116048225929039032299";
            Debug.Assert(TextNowSession.TryParse(acctInfo, out var session));
        }

        private static void TestTextNowRegisterInfoSerialization()
        {
            var textNowRegisterInfo = new TextNowRegisterInfo
            {
                BonusInfo = new BonusInfo
                {
                    AdvertisingIdInfo = Guid.NewGuid().ToString(),
                    AndroidId = AndroidHelpers.RandomAndroidId(),
                    BatteryLevel = "0.50",
                    BluetoothMacAddress = NetHelpers.RandomMacAddress(),
                    Country = "US",
                    IsUserAMonkey = "false",
                    Language = "en",
                    ScreenHeight = "123",
                    ScreenWidth = "456",
                    TelephonyManagerDeviceId = "null",
                    TimeZone = "America/Los_Angelas",
                    TelephonyManagerSimSerialNumber = "12512515",
                    UptimeDdHhMm = "0 4 33",
                    WiFiMacAddress = NetHelpers.RandomMacAddress()
                },
                Email = "hello@world.com",
                Password = "asdfasdfdfsdfs"
            };

            var test = JsonConvert.SerializeObject(textNowRegisterInfo, Formatting.None);
            Debug.Assert(test != null);
        }
    }
}
