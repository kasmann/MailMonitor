using System;
using System.Collections.Generic;
using System.Linq;

namespace MailMonitor
{
    public class EmailSettings
    {
        public string Login { get; }
        public string Password { get; }
        public string Server { get; }
        public int Port { get; }
        public List<MonitoringSettings> MonitoringSettingsList { get; }

        public EmailSettings(string login, string password, string server, int port)
        {
            if (!LoginValidated(login))
            {
                throw new IncorrectLoginException("Некорректное имя учетной записи.");
            }

            Login = login;
            Password = password;
            Server = server;
            Port = port;
            
            MonitoringSettingsList.Add(new MonitoringSettings("test", true, false, false, true));
            MonitoringSettingsList.Add(new MonitoringSettings("test2", false, true, false, false));
        }

        private static bool LoginValidated(string login)
        {
            return (login.Count(x => x == '@') == 1 && login.Count(x => x == '.') > 0);
        }
    }
}