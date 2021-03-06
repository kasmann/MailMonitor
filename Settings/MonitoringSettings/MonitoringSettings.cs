﻿using System;
using Newtonsoft.Json;

namespace MailMonitor.Settings.MonitoringSettings
{
    public class MonitoringSettings
    {
        public string EmailPart { get; }
        public string Condition { get; }
        public bool Notify { get; }
        public bool CopyTo { get; }
        public bool Forward { get; }
        public string ForwardTo { get; }
        public bool Print { get; }
        
        private readonly EmailPartEnum _emailPartEnum;        

        [JsonConstructor]
        public MonitoringSettings(string condition, string emailPart, bool notify = false, bool copyTo = false, bool forward = false, string forwardTo = null, bool print = false)
        {
            if (string.IsNullOrEmpty(condition)) throw new ArgumentException("Условие проверки не может быть пустым!");

            Condition = condition;

            if (Enum.TryParse(emailPart, true, out EmailPartEnum result))
            {
                EmailPart = emailPart;
                _emailPartEnum = result;
            }
            else
            {
                throw new ArgumentException($"Значение EmailPart {emailPart} недопустимо.");
            }
            
            Notify = notify;
            CopyTo = copyTo;
            Forward = forward;
            ForwardTo = forwardTo;
            Print = print;
        }

        public static MonitoringSettings CreateSample()
        {
            return new MonitoringSettings("condition", "title", false, false, false, "forward to", false);
        }
    }

    
}