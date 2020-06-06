using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailMonitor
{
    public class ProgramSettingsManager
    {
        public void LoadSettings(ProgramSettings programSettings)
        {
            var settingsFileFullPath = programSettings.settingsFileFullPath;
            if (!File.Exists(settingsFileFullPath))
            {
                throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {settingsFileFullPath} не найден.");
            }

            var settingsJson = File.ReadAllText(settingsFileFullPath);

            if (string.IsNullOrEmpty(settingsJson))
            {
                throw new SettingsFileEmptyOrNotFoundException($"Файл настроек {settingsFileFullPath} пуст.");
            }

            var jsonParsed = JObject.Parse(settingsJson).ToObject<ProgramSettings>();
            if (jsonParsed != null)
            {
                programSettings.EmailSettingsList = jsonParsed.EmailSettingsList;
            }
            else
            {
                Console.WriteLine("incorrect json");
            }
        }

        public void ParseCommandLine(ProgramSettings settings, string[] args)
        {
            var settingsArg = "";
            foreach (var arg in args.Where(arg => arg.Contains("settings=")))
            {
                settingsArg = arg.Substring(arg.IndexOf('=') + 1);
            }

            var settingsFileFullPath = string.IsNullOrEmpty(settingsArg)
                ? Properties.Resources.ResourceManager.GetString("SettingsFilename")
                : settingsArg;

            settings.settingsFileFullPath = settingsFileFullPath;

            var concurrentArg = "";
            foreach (var arg in args.Where(arg => arg.Contains("threads=") || arg.Contains("concurrent=")))
            {
                concurrentArg = arg.Substring(arg.IndexOf('=') + 1);
            }

            if (concurrentArg != "" && int.TryParse(concurrentArg, out var result))
                settings.maxConcurrent = result;


            var logFilePath = "";
            foreach (var arg in args.Where(arg => arg.Contains("logfile=")))
            {
                logFilePath = arg.Substring(arg.IndexOf('=') + 1);
            }

            if (logFilePath.Trim() != "" && !Path.GetInvalidPathChars().Any(logFilePath.Contains))
            {
                settings.log = "file";
                settings.logFileFullPath = logFilePath;
            }
            else
            {
                settings.log = "console";
            }

        }


        public void CreateSampleSettingsFile(string settingsFileFullPath)
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