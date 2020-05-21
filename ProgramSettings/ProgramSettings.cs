using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailMonitor
{
    public class ProgramSettings
    {
        public List<EmailSettings> EmailSettingsList { get; }

        [JsonConstructor]
        public ProgramSettings(List<EmailSettings> emailSettingsList)
        {
            if (emailSettingsList == null || emailSettingsList.Count <= 0)
            {
                throw new SettingsListEmptyException($"Список настроек в файле пуст.");
            }

            EmailSettingsList = emailSettingsList;
        }
    }
}
