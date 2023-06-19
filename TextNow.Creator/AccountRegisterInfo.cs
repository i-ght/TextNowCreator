using TextNow.Waifu;

namespace TextNow.Creator
{
    internal class AccountRegisterInfo
    {
        public AccountRegisterInfo(
            string email,
            string username,
            string password,
            TextNowAndroidDevice device)
        {
            Email = email;
            Username = username;
            Password = password;
            Device = device;
        }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; }
        public TextNowAndroidDevice Device { get; }
    }
}
