using System;
using System.IO;
using Newtonsoft.Json;

namespace MailMonitor
{
    internal static class Program
    {
        private static MonitoringJobExecutor _monitoringJobExecutor;

        private static void Main(string[] args)
        {
            var settings = new ProgramSettings();
            var manager = new ProgramSettingsManager();

            manager.ParseCommandLine(settings, args);

            if (!LoadSettings(manager, settings))
            {
                manager.CreateSampleSettingsFile(settings.settingsFileFullPath);
                return;
            }

            IActionQueue actionQueue = new ActionQueue();
            _monitoringJobExecutor = new MonitoringJobExecutor(actionQueue);
            
            var actionsCount = settings.EmailSettingsList.Count;
            var maxConcurrent = settings.maxConcurrent == 0 ? actionsCount : settings.maxConcurrent;
            _monitoringJobExecutor.Start(maxConcurrent);

            IProcessingActionsManager processingActionsManager;
            
            if (settings.log == "file")
            {
                try
                {
                    processingActionsManager = new LogfileProcessingActionsManager(settings.logFileFullPath);
                }
                //допустимо ли так отлавливать все исключения, выбрасываемые моим же методом, если их обработка - это только вывод сообщения в консоль?
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            else
            {
                processingActionsManager = new ConsoleProcessingActionsManager();
            }

            foreach (var setting in settings.EmailSettingsList)
            {
                var monitoringJob = new MonitoringJob(setting, processingActionsManager);
                actionQueue.Enqueue(monitoringJob.StartMonitoring);
            }

            while (_monitoringJobExecutor.IsRunning)
            {
                if (Console.ReadKey() != default)
                    _monitoringJobExecutor.Stop();
            }
        }           

        private static bool LoadSettings(ProgramSettingsManager manager, ProgramSettings settings)
        {
            try
            {
                manager.LoadSettings(settings);
                return true;
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
            catch (JsonReaderException)
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

            return false;
        }
    }
}
