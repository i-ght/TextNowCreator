using System;
using DankWaifu.Android;
using DankWaifu.Celly;
using DankWaifu.Collections;
using DankWaifu.Net;
using DankWaifu.Sys;

namespace TextNow.Waifu
{
    public class TextNowAndroidDevice
    {
        public TextNowAndroidDevice(
            string manufacturer,
            string model,
            string osVersion,
            int width,
            int height,
            string build,
            string advertisingId,
            string bluetoothMacAddress,
            string wifiMacAddress,
            string imeid,
            string androidId,
            string timezone,
            string iccid)
        {
            Manufacturer = manufacturer;
            Model = model;
            OsVersion = osVersion;
            Width = width;
            Height = height;
            Build = build;
            AdvertisingId = advertisingId;
            BluetoothMacAddress = bluetoothMacAddress;
            WifiMacAddress = wifiMacAddress;
            Imeid = imeid;
            AndroidId = androidId;
            TimeZone = timezone;
            Iccid = iccid;
        }

        public string Manufacturer { get; }
        public string Model { get; }
        public string OsVersion { get; }
        public int Width { get; }
        public int Height { get; }
        public string Build { get; }
        public string AdvertisingId { get; }
        public string BluetoothMacAddress { get; }
        public string WifiMacAddress { get; }
        public string Imeid { get; }
        public string AndroidId { get; }
        public string TimeZone { get; }
        public string Iccid { get; }

        public override string ToString()
        {
            return
                $"{Manufacturer}|{Model}|{OsVersion}|{Width}|{Height}|{Build}|{AdvertisingId}|{BluetoothMacAddress}|{WifiMacAddress}|{Imeid}|{AndroidId}|{TimeZone}|{Iccid}"
                    .Replace(":", ";");
        }

        public static bool TryParse(string input, out TextNowAndroidDevice device)
        {
            device = null;
            if (string.IsNullOrWhiteSpace(input) ||
                !input.Contains("|"))
            {
                return false;
            }

            input = input.Replace(";", ":");

            var split = input.Split('|');
            if (StringHelpers.AnyNullOrEmpty(split))
                return false;

            switch (split.Length)
            {
                case 13:
                {
                    if (!int.TryParse(split[3], out var width))
                        return false;

                    if (!int.TryParse(split[4], out var height))
                        return false;

                    var manufacturer = split[0];
                    var model = split[1];
                    var osVersion = split[2];
                    var build = split[5];
                    var advertisingId = split[6];
                    var bluetoothMac = split[7];
                    var wifiMac = split[8];
                    var imeid = split[9];
                    var androidId = split[10];
                    var timezone = split[11];
                    var iccid = split[12];

                    device = new TextNowAndroidDevice(
                        manufacturer,
                        model,
                        osVersion,
                        width,
                        height,
                        build,
                        advertisingId,
                        bluetoothMac,
                        wifiMac,
                        imeid,
                        androidId,
                        timezone,
                        iccid
                    );

                    return true;
                }
                case 6:
                {
                    var manufacturer = split[0];
                    var model = split[1];
                    var osVersion = split[2];
                    var widthStr = split[3];
                    var heightStr = split[4];
                    var build = split[5];

                    if (!int.TryParse(widthStr, out var width))
                        return false;

                    if (!int.TryParse(heightStr, out var height))
                        return false;

                    var advertisingId = Guid.NewGuid().ToString();
                    var bluetoothMacAddr = NetHelpers.RandomMacAddress();
                    var wifiMacAddr = NetHelpers.RandomMacAddress();
                    var imeid = CellyHelpers.RandomImei().Substring(0, 14);
                    var androidId = AndroidHelpers.RandomAndroidId();
                    var timezone = RandomTimeZoneId();
                    var iccid = CellyHelpers.RandomIccid(CellyHelpers.RandomCellyCarrierInfo());

                    device = new TextNowAndroidDevice(
                        manufacturer,
                        model,
                        osVersion,
                        width,
                        height,
                        build,
                        advertisingId,
                        bluetoothMacAddr,
                        wifiMacAddr,
                        imeid,
                        androidId,
                        timezone,
                        iccid
                    );
                    return true;
                }

                default:
                    return false;
            }
        }

        private static readonly string[] TimeZoneIds =
            {"America/Denver", "America/New_York", "America/Chicago", "America/Los_Angeles"};

        private static string RandomTimeZoneId()
        {
            return TimeZoneIds.RandomSelection();
        }
    }
}
