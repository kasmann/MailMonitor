using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailMonitor.Settings.ProgramSettings
{
    public class ProgramSettings
    {
        [JsonIgnore] public string settingsFileFullPath { get; set; }
        [JsonIgnore] public int maxConcurrent { get; set; }
        [JsonIgnore] public string log { get; set; }
        [JsonIgnore] public string logFileFullPath { get; set; }
        public IList<EmailSettings> EmailSettingsList { get; set; }

        public ProgramSettings() {}

        [JsonConstructor]
        public ProgramSettings(IList<EmailSettings> emailSettingsList)
        {
            if (emailSettingsList == null || emailSettingsList.Count <= 0)
            {
                throw new SettingsListEmptyException($"Список настроек в файле пуст.");
            }

            EmailSettingsList = emailSettingsList;
        }
    }
}
