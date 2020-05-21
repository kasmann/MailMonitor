using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailMonitor
{
    public class ProgramSettingsManager
    {        
        public static ProgramSettings LoadSettings(string settingsFileFullPath)
        {
            if (!File.Exists(settingsFileFullPath))
            {
                throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {settingsFileFullPath} не найден.");
            }

            var settingsJson = File.ReadAllText(settingsFileFullPath);

            if (string.IsNullOrEmpty(settingsJson))
            {
                throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {settingsFileFullPath} пуст.");
            }
            
            var settings = JObject.Parse(settingsJson).ToObject<ProgramSettings>();
            
            return settings;
        }

        public static void CreateSampleSettingsFile(string settingsFileFullPath)
        {
            var defaultEmailSettingsList = new List<EmailSettings> { EmailSettings.CreateSample() };
            var defaultSettings = new ProgramSettings(defaultEmailSettingsList);

            try
            {
                File.WriteAllText(settingsFileFullPath, JObject.FromObject(defaultSettings).ToString());
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Директория файла настроек не найдена. Завершение программы.");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine($"Путь к файлу настроек слишком длинный. Завершение программы.");
            }
            catch (UnauthorizedAccessException uaException)
            {
                Console.WriteLine(
                    $"Недостаточно прав для создания файла настроек по умолчанию. Завершение программы.\n{uaException.Message}");
            }
            catch (IOException ioException)
            {
                Console.WriteLine(
                    $"Ошибка создания или записи файла настроек по умолчанию. Завершение программы.\n{ioException.Message}");
            }

            Console.WriteLine("Создан образец файла настроек. Измените настройки и перезапустите программу.");
        }
    }
}