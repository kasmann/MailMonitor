﻿using System;

namespace MailMonitor
{
    public class SettingsFileEmptyOrNotFoundException : Exception
    {
        public SettingsFileEmptyOrNotFoundException(string message) : base(message) { }
    }

    public class SettingsListEmptyException : Exception
    {
        public SettingsListEmptyException(string message) : base(message) { }
    }

    public class JobExecutorStoppedException : Exception
    {
        public JobExecutorStoppedException(string message) : base(message) { }
    }

    public class JobExecutorStartedException : Exception
    {
        public JobExecutorStartedException(string message) : base(message) { }
    }

}