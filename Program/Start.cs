using System;
using MailMonitor.ActionQueue;
using MailMonitor.MessageAgents;
using MailMonitor.Monitoring;
using MailMonitor.Settings.ProgramSettings;
using Microsoft.Extensions.Logging;

namespace MailMonitor.Program
{
    internal partial class Program
    {
        private static MonitoringJobExecutor _monitoringJobExecutor;
        private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;
        
        private static void Start(string[] args)
        { 
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _loggerFactory.CreateLogger<Program>();

            var settings = new ProgramSettings();
            var manager = new ProgramSettingsManager(_logger);

            manager.ParseCommandLine(settings, args);

            if (!manager.LoadSettings(settings, out var createSample) && createSample)
            {
                manager.CreateSampleSettingsFile();
                return;
            }
            
            IActionQueue actionQueue = new ActionQueue.ActionQueue();
            IActionQueue stopActionQueue = new ActionQueue.ActionQueue();
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
                var monitoringJob = new Monitoring.MonitoringJob(setting, processingActionsManager, _logger);
                actionQueue.Enqueue(monitoringJob.StartMonitoring);
                stopActionQueue.Enqueue(monitoringJob.Stop);
            }

            while (_monitoringJobExecutor.IsRunning)
            {
                Console.WriteLine("stopping works");
                if (Console.ReadKey() != default)
                    _monitoringJobExecutor.Stop(stopActionQueue);
            }
        }           

        private static void OnErrorOccured(object obj, MonitoringErrorEventArgs args)
        {
            _logger.LogError($"*****При выполнении задач мониторинга возникла ошибка: {args.Message}\n" +
                $"Метод, вызвавший ошибку: {args.Method}\n" +
                $"{args.ErrorMessage}");
        }
    }
}