using DankWaifu.Sys;

namespace TextNow.Waifu
{
    public class TextNowAccount
    {
        public TextNowAccount(string email, string username, string password, TextNowAndroidDevice device)
        {
            Email = email;
            Username = username;
            Password = password;
            Device = device;
        }

        public string Email { get; }
        public string Username { get; }
        public string Password { get; }
        public TextNowAndroidDevice Device { get; }

        public override string ToString()
        {
            return $"{Email}:{Username}:{Password}:{Device}";
        }

        public static bool TryParse(string input, out TextNowAccount account)
        {
            account = null;
            if (string.IsNullOrWhiteSpace(input) ||
                !input.Contains("|"))
            {
                return false;
            }

            var split = input.Split(':');
            if (split.Length != 4 ||
                StringHelpers.AnyNullOrWhitespace(split))
            {
                return false;
            }

            if (!TextNowAndroidDevice.TryParse(split[3], out var device))
                return false;

            var email = split[0];
            var username = split[1];
            var password = split[2];

            account = new TextNowAccount(email, username, password, device);

            return true;
        }
    }
}
