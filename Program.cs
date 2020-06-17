using System;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MailMonitor
{
    internal class Program
    {
        private static MonitoringJobExecutor _monitoringJobExecutor;
        private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;

        private static void Main(string[] args)
        {            
            Start(args);
            Console.ReadKey();
        }

        private static void Start(string[] args)
        { 
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _loggerFactory.CreateLogger<Program>();

            var settings = new ProgramSettings();
            var manager = new ProgramSettingsManager(_logger);

            manager.ParseCommandLine(settings, args);

            if (!LoadSettings(manager, settings))
            {
                manager.CreateSampleSettingsFile(settings.settingsFileFullPath);
                return;
            }

            IActionQueue actionQueue = new ActionQueue();
            _monitoringJobExecutor = new MonitoringJobExecutor(actionQueue, _logger);
            _monitoringJobExecutor.OnErrorOccured += OnErrorOccured;


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
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return;
                }
            }
            else
            {
                processingActionsManager = new ConsoleProcessingActionsManager();
            }

            foreach (var setting in settings.EmailSettingsList)
            {
                var monitoringJob = new MonitoringJob(setting, processingActionsManager, _logger);
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
            //try
            //{
            //    manager.LoadSettings(settings);
            //    return true;
            //}
            //catch (SettingsFileEmptyOrNotFoundException ex)
            //{
            //    _logger.LogError(ex.Message);
            //}
            //catch (SettingsListEmptyException ex)
            //{
            //    _logger.LogError(ex.Message);
            //}
            //catch (ArgumentException ex)
            //{
            //    _logger.LogError($"Возникла ошибка при чтении файла настроек.\n{ex.Message}\n{ex.ParamName}");
            //}
            //catch (JsonReaderException)
            //{
            //    _logger.LogError("Файл настроек имеет некорректную структуру.");
            //}
            //catch (IOException)
            //{
            //    _logger.LogError("Ошибка чтения файла настроек.");
            //}
            //catch (UnauthorizedAccessException)
            //{
            //    _logger.LogError("Недостаточно прав для доступа к файлу настроек.");
            //}

            //Console.ReadKey();
            //return false;

            return manager.LoadSettings(settings);
        }

        private static void OnErrorOccured(object obj, MonitoringErrorEventArgs args)
        {
            _logger.LogError($"*****При выполнении задач мониторинга возникла ошибка: {args.Message}\n" +
                $"Метод, вызвавший ошибку: {args.Method}\n" +
                $"{args.ErrorMessage}");
        }
    }
}
