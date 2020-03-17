using System;

namespace MailMonitor
{
    public class IncorrectLoginException : Exception
    {
        public IncorrectLoginException(string message) : base(message) {}
    }
}