using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailMonitor.Settings
{
    public class EmailSettings
    {
        public string Login { get; }        
        public string Password { get; }        
        public string Server { get; }        
        public int Port { get; }        
        public bool UseSSL { get; }        
        public int Timeout { get; }
        
        public List<MonitoringSettings.MonitoringSettings> MonitoringSettingsList { get; }

        [JsonConstructor]
        private EmailSettings(string login, string password, string server, int port, bool useSSL, int timeout, List<MonitoringSettings.MonitoringSettings> monitoringSettingsList)
        {
            if (monitoringSettingsList == null || monitoringSettingsList.Count <= 0)
            {
                throw new SettingsListEmptyException($"Список настроек для логина {login} пуст.");
            }
            
            if (string.IsNullOrEmpty(login.Trim()))
            {
                throw new ArgumentNullException(nameof(login), "В файле настроек не указан адрес почтового ящика.");
            }
            
            if (string.IsNullOrEmpty(password.Trim()))
            {
                throw new ArgumentNullException(nameof(password), $"Пароль для учетной записи {login} не может быть пустым.");
            }
            
            if (string.IsNullOrEmpty(server.Trim()))
            {
                throw new ArgumentNullException(nameof(server), "В файле настроек не указан адрес почтового сервера.");
            }
            
            MonitoringSettingsList = monitoringSettingsList;
            Login = login;
            Password = password;
            Server = server;
            Port = port == 0 ? 993 : port;
            UseSSL = useSSL;
            Timeout = timeout == 0 ? 120000 : timeout;
        }

        public static EmailSettings CreateSample()
        {
            var defaultMonitoringSettingsList = new List<MonitoringSettings.MonitoringSettings>
            {
                MonitoringSettings.MonitoringSettings.CreateSample()
            };
            
            var defaultEmailSettings = new EmailSettings(
                "login@server", 
                "password", 
                "server", 
                999, 
                false,
                999, 
                defaultMonitoringSettingsList);
            
            return defaultEmailSettings;
        }
    }
}