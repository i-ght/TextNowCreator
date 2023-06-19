using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DankWaifu.Captcha;
using DankWaifu.Collections;
using DankWaifu.Net;
using DankWaifu.Sys;
using TextNow.Waifu;
using TextNow.Waifu.Json;

namespace TextNow.Creator
{
    internal class TextNowCreatorWorker : Mode
    {
        private readonly Collections _collections;
        private readonly Stats _stats;
        private readonly SemaphoreSlim _writeLock;

        public TextNowCreatorWorker(
            int index,
            DataGridItem ui,
            Collections collections,
            Stats stats,
            SemaphoreSlim writeLock) : base(index, ui)
        {
            _collections = collections;
            _stats = stats;
            _writeLock = writeLock;
        }

        private string GenerateUsername()
        {
            var firstName = _collections.FirstNames.GetNext();
            var lastName = _collections.LastNames.GetNext();
            if (StringHelpers.AnyNullOrEmpty(firstName, lastName))
                throw new InvalidOperationException("failed to generate a username");

            firstName = StringHelpers.ReplaceNonAlphaNumerics(firstName);
            lastName = StringHelpers.ReplaceNonAlphaNumerics(lastName);
            if (StringHelpers.AnyNullOrEmpty(firstName, lastName))
                throw new InvalidOperationException("failed to generate a username");

            return $"{firstName}{lastName}".ToLower();
        }

        private AccountRegisterInfo CreateAccountRegisterInfo(
            string username)
        {
            var password = $"{_collections.FirstNames.GetNext().ToLower()}{_collections.LastNames.GetNext().ToLower()}";
            var ret = new AccountRegisterInfo(
                $"{username}@{EmailHelpers.RandomEmailDomain()}",
                username,
                password,
                AndroidDevices.Devices.GetNext()
            );
            return ret;
        }

        private static string RandomUptime()
        {
            return $"{RandomHelpers.RandomInt(0, 10)} {RandomHelpers.RandomInt(1, 25)} {RandomHelpers.RandomInt(1, 60)}";
        }

        private static TextNowRegisterInfo CreateTextNowRegisterInfo(
            AccountRegisterInfo regInfo)
        {
            var txtNowRegInfo = new TextNowRegisterInfo
            {
                BonusInfo = new BonusInfo
                {
                    AdvertisingIdInfo = regInfo.Device.AdvertisingId,
                    AndroidId = regInfo.Device.AndroidId,
                    BatteryLevel = $"0.{RandomHelpers.RandomInt(40, 90)}",
                    BluetoothMacAddress = regInfo.Device.BluetoothMacAddress,
                    Country = "US",
                    IsUserAMonkey = "false",
                    Language = "en",
                    ScreenHeight = regInfo.Device.Height.ToString(),
                    ScreenWidth = regInfo.Device.Width.ToString(),
                    TelephonyManagerDeviceId = regInfo.Device.Imeid,
                    TimeZone = regInfo.Device.TimeZone,
                    TelephonyManagerSimSerialNumber = regInfo.Device.Iccid,
                    WiFiMacAddress = regInfo.Device.WifiMacAddress,
                    UptimeDdHhMm = RandomUptime()
                },
                Email = regInfo.Email,
                Password = regInfo.Password
            };

            return txtNowRegInfo;
        }

        public override async Task BaseAsync()
        {
            while (_stats.Created < Settings.Get<int>(Constants.MaxCreates))
            {
                try
                {
                    var username = GenerateUsername();
                    var regInfo = CreateAccountRegisterInfo(username);
                    UI.Account = regInfo.Email;
                    Interlocked.Increment(ref _stats.Attempts);

                    var cfg = new HttpWaifuConfig
                    {
                        Proxy = _collections.Proxies.GetNext()
                    };
                    var httpClient = new HttpWaifu(cfg);
                    var loggedOutClient = new TextNowClientLoggedOut(httpClient, regInfo.Device);

                    await FindAvailableEmailToRegisterWith(loggedOutClient, regInfo)
                        .ConfigureAwait(false);

                    var suggestedUsername = await RequestUsernameSuggestion(loggedOutClient, regInfo.Username)
                        .ConfigureAwait(false);

                    await EnsureSuggestedUsernameIsAvailable(loggedOutClient, suggestedUsername)
                        .ConfigureAwait(false);

                    regInfo.Username = suggestedUsername;

                    UI.Status = "Attempting registration: ...";
                    var textNowRegInfo = CreateTextNowRegisterInfo(regInfo);
                    var registerResponse = await loggedOutClient.CreateAccount(regInfo.Username, textNowRegInfo)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(registerResponse.Result.CaptchaLink))
                    {
                        var captchaLink = Regex.Unescape(registerResponse.Result.CaptchaLink);
                        if (!await HandleREEEECaptcha(captchaLink, loggedOutClient, regInfo.Device)
                            .ConfigureAwait(false))
                        {
                            continue;
                        }
                    }

                    var account = new TextNowAccount(
                        regInfo.Email,
                        regInfo.Username,
                        regInfo.Password,
                        regInfo.Device
                    );
                    var session = new TextNowSession(
                        registerResponse.Result.Id,
                        account)
                    ;
                    var clientLoggedIn = new TextNowClientLoggedIn(
                        httpClient,
                        session
                    );
                    var proxy = clientLoggedIn.Proxy;

                    var acctType = TextNowAccountType.DoesNotHavePhoneNumber;

                    try
                    {
                        var reservation = await AcquirePhoneNumberReservation(clientLoggedIn)
                            .ConfigureAwait(false);

                        var acquiredPhoneNumber = await AcquirePhoneNumber(
                            clientLoggedIn,
                            reservation.Result.ReservationId,
                            reservation.Result.PhoneNumbers
                        ).ConfigureAwait(false);

                        acctType = TextNowAccountType.HasPhoneNumber;

                        session = new TextNowSession(
                            session.ClientId,
                            acquiredPhoneNumber.Result.PhoneNumber,
                            session.Account
                        );

                        clientLoggedIn.Proxy = proxy;
                        await clientLoggedIn.RetrieveUserInfo()
                            .ConfigureAwait(false);
                        await clientLoggedIn.RetrieveMessages()
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await SaveSession(session, acctType)
                            .ConfigureAwait(false);

                        Interlocked.Increment(ref _stats.Created);
                        await UpdateThreadStatusAsync("Account created", 1000)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    await OnExceptionAsync(e)
                        .ConfigureAwait(false);
                }
            }

            UI.Account = string.Empty;
            UI.Status = string.Empty;
        }

        private async Task<bool> HandleREEEECaptcha(string captchaLink, TextNowClientLoggedOut client, TextNowAndroidDevice device)
        {
            UI.Status = "Waiting for recaptcha solution from 2captcha: ...";
            const string textNowRecaptchaKey = "6LcU4gkTAAAAAM4SyYTmXlKvqwWLuaFLy-30rzBn";
            var caplient = new ThreeCaptcha5Me(Settings.Get<string>(Constants.TwoCaptchaAPIKey));
            var solution = await caplient.REEEEEEECaptcha(
                textNowRecaptchaKey,
                captchaLink,
                TimeSpan.FromSeconds(Settings.Get<int>(Constants.TwoCaptchaTimeout))
            ).ConfigureAwait(false);

            UI.Status = "Validating captcha solution: ...";
            for (var i = 0; i < 4; i++)
            {
                var validateProxy = _collections.ValidateCaptchaProxies.GetNext();
                if (await client.RetrieveValidationOfCaptchaSolution(
                    captchaLink,
                    solution,
                    device,
                    validateProxy
                ).ConfigureAwait(false))
                {
                    return true;
                }

                await Task.Delay(500)
                    .ConfigureAwait(false);
            }

            await UpdateThreadStatusAsync("Failed to validate captcha solution.", 5000)
                .ConfigureAwait(false);
            return false;
        }

        private async Task<string> RequestUsernameSuggestion(TextNowClientLoggedOut client, string username)
        {
            UI.Status = "Requesting username suggestion: ...";
            var usernameSuggestion = await client.RetrieveUsernameSuggestion(username)
                .ConfigureAwait(false);
            return usernameSuggestion;
        }

        private async Task FindAvailableEmailToRegisterWith(TextNowClientLoggedOut client, AccountRegisterInfo regInfo)
        {
            UI.Status = "Finding available email to register: ...";

            for (var i = 0; i < 3; i++)
            {
                if (await client.RetrieveEmailAvailability(regInfo.Email)
                    .ConfigureAwait(false))
                {
                    return;
                }

                var email = regInfo.Email;
                var split = email.Split('@');
                email = $"{email}{RandomHelpers.RandomInt(9)}@{split[1]}";
                regInfo.Username = email.Split('@')[0];
                regInfo.Email = email;
            }

            throw new InvalidOperationException("Failed to find an available email address to register with");
        }

        private async Task EnsureSuggestedUsernameIsAvailable(TextNowClientLoggedOut client, string username)
        {
            UI.Status = "Ensuring username is available: ...";
            if (await client.RetrieveUsernameAvailability(username)
                .ConfigureAwait(false))
            {
                return;
            }

            throw new InvalidOperationException($"{username} is not available for registration");
        }

        private async Task<TextNowPhoneNumberReservationResponse> AcquirePhoneNumberReservation(
            TextNowClientLoggedIn client)
        {
            var attempts = 0;
            while (attempts++ < Settings.Get<int>(Constants.MaxReservePhoneNumberAttempts))
            {
                UI.Status = "Getting a phone number reservation: ...";

                try
                {
                    if (_collections.ReservePhoneNumberProxies.Count > 0)
                        client.Proxy = _collections.ReservePhoneNumberProxies.GetNext();
                    var areaCode = _collections.AreaCodes.GetNext();

                    var reservation = await client.CreatePhoneNumberReservation(areaCode)
                        .ConfigureAwait(false);

                    return reservation;
                }
                catch (InvalidOperationException ex)
                {
                    await UpdateThreadStatusAsync(
                        $"Attempting to reserve phone number: FAILED ({ex.GetType().Name}~{ex.Message})",
                        5000
                    ).ConfigureAwait(false);
                }
            }

            throw new InvalidOperationException(
                "failed to reserve a phone number after 5 attempts"
            );
        }

        private async Task<TextNowAssignReservedResponse> AcquirePhoneNumber(
            TextNowClientLoggedIn client,
            string reservationId,
            ICollection<string> phoneNumbers)
        {
            var phoneNumber = phoneNumbers.RandomSelection();
            UI.Status = $"Acquiring phone number {phoneNumber}: ...";

            var response = await client.CreatePhoneNumberAssignment(
                reservationId,
                phoneNumber
            ).ConfigureAwait(false);
            return response;
        }

        private enum TextNowAccountType
        {
            DoesNotHavePhoneNumber,
            HasPhoneNumber
        }

        private async Task SaveSession(
            TextNowSession session,
            TextNowAccountType accountType)
        {
            await _writeLock.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                string filename;
                switch (accountType)
                {
                    case TextNowAccountType.HasPhoneNumber:
                        filename = "textnow-accounts_created.txt";
                        break;

                    default:
                        filename = "textnow-accounts_created-no_phone_number.txt";
                        break;
                }

                using (var streamWriter = new StreamWriter(filename, true))
                {
                    await streamWriter.WriteLineAsync(
                        session.ToString()
                    ).ConfigureAwait(false);
                }
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}