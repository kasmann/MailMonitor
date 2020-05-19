using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailMonitor
{
    public class ProgramSettings
    {
        [JsonIgnore]
        public static ProgramSettings Settings { get; private set; }
        public List<EmailSettings> EmailSettingsList { get; }

        private const string SettingsFilename = "settings.json";

        [JsonConstructor]
        private ProgramSettings(List<EmailSettings> emailSettingsList)
        {
            if (emailSettingsList == null || emailSettingsList.Count <= 0)
            {
                throw new SettingsListEmptyException($"Список настроек в файле {SettingsFilename} пуст.");
            }
           
            EmailSettingsList = emailSettingsList;
        }
        
        public static bool SettingsLoaded()
        {
            try
            {
                Settings = LoadSettings();
            }
            catch (SettingsFileEmptyOrNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                try
                {
                    CreateSampleSettingsFile();
                    Console.WriteLine(
                        "Создан образец файла настроек. Измените настройки и перезапустите программу.");
                    return false;
                }
                catch (IOException ioException)
                {
                    Console.WriteLine(
                        $"Ошибка создания или записи файла настроек по умолчанию.\n{ioException.Message}");
                    return false;
                }
                catch (UnauthorizedAccessException uaException)
                {
                    Console.WriteLine(
                        $"Недостаточно прав для создания файла настроек по умолчанию.\n{uaException.Message}");
                    return false;
                }
            }
            catch (SettingsListEmptyException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Возникла ошибка при чтении файла настроек.\n{ex.Message}\n{ex.ParamName}");
                return false;
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("Файл настроек имеет некорректную структуру.");
                return false;
            }
            catch (IOException)
            {
                Console.WriteLine("Ошибка чтения файла настроек.");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Недостаточно прав для доступа к файлу настроек.");
                return false;
            }
            
            return true;
        }
        
        private static ProgramSettings LoadSettings()
        {
            if (!File.Exists("settings.json")) throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {SettingsFilename} не найден.");

            var settingsJson = File.ReadAllText(SettingsFilename);
            
            if (string.IsNullOrEmpty(settingsJson)) throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {SettingsFilename} пуст.");
            
            var settings = JObject.Parse(settingsJson).ToObject<ProgramSettings>();
            
            return settings;
        }

        private static void CreateSampleSettingsFile()
        {
            var defaultEmailSettingsList = new List<EmailSettings> {EmailSettings.CreateSample()};
            var defaultSettings = new ProgramSettings(defaultEmailSettingsList);
            
            File.WriteAllText("settings.json", JObject.FromObject(defaultSettings).ToString());
        }
    }
}