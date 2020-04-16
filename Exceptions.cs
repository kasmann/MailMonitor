using System;

namespace MailMonitor
{
    public class SettingsFileEmptyOrNotFoundException : Exception
    {
        public SettingsFileEmptyOrNotFoundException(string message) : base(message) { }
    }

    public class SettingsListEmpty : Exception
    {
        public SettingsListEmpty(string message) : base(message) { }
    }

    public class JobExecutorStopped : Exception
    {
        public JobExecutorStopped(string message) : base(message) { }
    }

    public class JobExecutorStarted : Exception
    {
        public JobExecutorStarted(string message) : base(message) { }
    }

}