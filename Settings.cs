using System;
using System.Collections.Generic;

namespace MailMonitor
{
    public class Settings : IDisposable
    {
        public List<EmailSettings> EmailSettingsList { get; set; }
        
        public void Dispose()
        {
            
        }
    }
}