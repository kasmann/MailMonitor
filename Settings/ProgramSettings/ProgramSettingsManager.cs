using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailMonitor.Validators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MailMonitor.Settings.ProgramSettings
{
    public class ProgramSettingsManager
    {
        private readonly ILogger _logger;

        public ProgramSettingsManager(ILogger logger)
        {
            _logger = logger;
        }

        public bool LoadSettings(ProgramSettings programSettings, out bool createSample)
        {
            createSample = false;
            var settingsFileFullPath = programSettings.settingsFileFullPath;

            var fileAvailabilityValidator = new SettingsFileAvailabilityValidator();
            var validationResult = fileAvailabilityValidator.Validate(settingsFileFullPath);
            
            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    _logger.LogError(failure.ErrorMessage);
                }
                createSample = true;
                return false;
            }
            
            var fileNotEmptyValidator = new FileNotEmptyValidator();
            validationResult = fileNotEmptyValidator.Validate(settingsFileFullPath);
            
            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    _logger.LogError(failure.ErrorMessage);
                }
                createSample = true;
                return false;
            }
            
            var jsonTextValidator = new JsonTextValidator();
            validationResult = jsonTextValidator.Validate(settingsFileFullPath);
            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    _logger.LogError(failure.ErrorMessage);
                }
                return false;
            }

            var settingsJson = File.ReadAllText(settingsFileFullPath);
            try
            {
                var jsonParsed = JObject.Parse(settingsJson).ToObject<ProgramSettings>();
                programSettings.EmailSettingsList = jsonParsed.EmailSettingsList;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public void ParseCommandLine(ProgramSettings settings, string[] args)
        {
            var settingsArg = "";
            foreach (var arg in args.Where(arg => arg.ToLower().Contains("settings=")))
            {
                settingsArg = arg.Substring(arg.IndexOf('=') + 1);
            }

            var settingsFileFullPath = (string.IsNullOrEmpty(settingsArg) || Path.GetInvalidPathChars().Any(settingsArg.Contains))
                ? Properties.Resources.ResourceManager.GetString("SettingsFilename")
                : settingsArg;

            settings.settingsFileFullPath = settingsFileFullPath;

            var concurrentArg = "";
            foreach (var arg in args.Where(arg => arg.ToLower().Contains("threads=") || arg.Contains("concurrent=")))
            {
                concurrentArg = arg.Substring(arg.IndexOf('=') + 1);
            }

            if (concurrentArg != "" && int.TryParse(concurrentArg, out var result))
                settings.maxConcurrent = result;


            var logFilePath = "";
            foreach (var arg in args.Where(arg => arg.ToLower().Contains("logfile=")))
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

        public void CreateSampleSettingsFile()
        {
            var defaultEmailSettingsList = new List<EmailSettings> {EmailSettings.CreateSample()};
            var defaultSettings = new ProgramSettings(defaultEmailSettingsList);
            var defaultSettingsFilePath = Properties.Resources.ResourceManager.GetString("SettingsFilename");

            var fileAvailabilityValidator = new SettingsFileAvailabilityValidator();
            var validationResult = fileAvailabilityValidator.Validate(defaultSettingsFilePath);

            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    _logger.LogError(failure.ErrorMessage);
                }
            }

            File.WriteAllText(defaultSettingsFilePath, JObject.FromObject(defaultSettings).ToString());
            _logger.LogInformation(
                $"Создан образец файла настроек {defaultSettingsFilePath}. Измените настройки и перезапустите программу.");
        }
        
    }
}