using System;

namespace TextNow.Waifu
{
    public class TextNowException : InvalidOperationException
    {
        public TextNowException(string message) : base(message) { }
    }
}
