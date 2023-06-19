using System.Collections.Concurrent;
using System.Net;
using DankWindowsWaifu.WPF;

namespace TextNow.Creator
{
    internal class Collections
    {
        private readonly SettingsDataGrid _settings;

        public Collections(
            ConcurrentQueue<WebProxy> proxies,
            ConcurrentQueue<WebProxy> reservePhoneNumberProxies,
            ConcurrentQueue<WebProxy> validateCaptchaProxies,
            SettingsDataGrid settings)
        {
            Proxies = proxies;
            ReservePhoneNumberProxies = reservePhoneNumberProxies;
            ValidateCaptchaProxies = validateCaptchaProxies;
            _settings = settings;
        }

        public ConcurrentQueue<WebProxy> Proxies { get; }
        public ConcurrentQueue<WebProxy> ReservePhoneNumberProxies { get; }
        public ConcurrentQueue<WebProxy> ValidateCaptchaProxies { get; }
        public ConcurrentQueue<string> FirstNames => _settings.GetConcurrentQueue(Constants.FirstNames);
        public ConcurrentQueue<string> LastNames => _settings.GetConcurrentQueue(Constants.LastNames);
        public ConcurrentQueue<string> AreaCodes => _settings.GetConcurrentQueue(Constants.AreaCodes);
    }
}
