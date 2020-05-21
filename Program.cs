using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace MailMonitor
{
    internal static class Program
    {
        private static MonitoringJobExecutor _monitoringJobExecutor;

        private static void Main(string[] args)
        {
            string settingsFileFullPath = DefineFullPath(args);            

            ProgramSettings settings = LoadSettings(settingsFileFullPath);

            if (settings is null)
            {
                ProgramSettingsManager.CreateSampleSettingsFile(settingsFileFullPath);
                return;
            }

            ActionQueue actionQueue = new ActionQueue();
            foreach (var setting in settings.EmailSettingsList)
            {
                var monitoringJob = new MonitoringJob(setting);
                actionQueue.Add(monitoringJob.StartMonitoring);
            }

            _monitoringJobExecutor = new MonitoringJobExecutor(actionQueue);
            _monitoringJobExecutor.Start();

            while (_monitoringJobExecutor.IsRunning)
            {
                //прекращение работы по нажатию любой клавиши в консоли
                if (Console.ReadKey() != default)
                    _monitoringJobExecutor.Stop();
            }
        }           

        private static string DefineFullPath(string[] args)
        {
            string commandLineArg = "";
            foreach (var arg in args.Where(arg => arg.Contains(".json")))
            {
                commandLineArg = arg;
            }

            string settingsFileFullPath = string.IsNullOrEmpty(commandLineArg)
                ? Properties.Resources.ResourceManager.GetString("SettingsFilename")
                : commandLineArg;

            return settingsFileFullPath;
        }

        public static ProgramSettings LoadSettings(string settingsFileFullPath)
        {
            ProgramSettings settings = null;

            try
            {
                settings = ProgramSettingsManager.LoadSettings(settingsFileFullPath);
            }
            catch (SettingsFileEmptyOrNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (SettingsListEmptyException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Возникла ошибка при чтении файла настроек.\n{ex.Message}\n{ex.ParamName}");
            }
            catch (JsonReaderException ex)
            {                
                Console.WriteLine("Файл настроек имеет некорректную структуру.");
            }
            catch (IOException)
            {                
                Console.WriteLine("Ошибка чтения файла настроек.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Недостаточно прав для доступа к файлу настроек.");
            }

            return settings;
        }
        
    }
}
