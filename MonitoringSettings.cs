using System;

namespace MailMonitor
{
    public class MonitoringSettings
    {
        public string Condition;
        public bool Notify;
        public bool CopyTo;
        public bool Forward;
        public bool Print;

        public MonitoringSettings(string condition, bool notify = false, bool copyTo = false, bool forward = false, bool print = false)
        {
            if (string.IsNullOrEmpty(condition)) throw new ArgumentException("Условие проверки не может быть пустым!");
            Notify = notify;
            CopyTo = copyTo;
            Forward = forward;
            Print = print;
        }
    }
}