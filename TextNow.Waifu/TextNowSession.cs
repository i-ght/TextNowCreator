using DankWaifu.Sys;

namespace TextNow.Waifu
{
    public class TextNowSession
    {
        public TextNowSession(string clientId, string phoneNumber, TextNowAccount account)
        {
            ClientId = clientId;
            PhoneNumber = phoneNumber;
            Account = account;
        }

        public TextNowSession(string clientId, TextNowAccount account)
        {
            ClientId = clientId;
            Account = account;
        }

        public string ClientId { get; }
        public string PhoneNumber { get;  }
        public TextNowAccount Account { get; }

        public override string ToString()
        {
            return $"{ClientId}:{PhoneNumber}:{Account}";
        }

        public static bool TryParse(string input, out TextNowSession session)
        {
            session = null;
            if (string.IsNullOrWhiteSpace(input) ||
                !input.Contains(":"))
            {
                return false;
            }

            var split = input.Split(':');
            if (split.Length != 6 ||
                StringHelpers.AnyNullOrWhitespace(split))
            {
                return false;
            }

            var accountSplits = new[]
            {
                split[2],
                split[3],
                split[4],
                split[5]
            };
            var acctStr = string.Join(":", accountSplits);

            if (!TextNowAccount.TryParse(acctStr, out var account))
                return false;

            var clientId = split[0];
            var phoneNumber = split[1];

            session = new TextNowSession(clientId, phoneNumber, account);
            return true;
        }
    }
}
