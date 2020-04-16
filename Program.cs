using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace MailMonitor
{
    internal static class Program
    {
        private static MonitoringJobExecutor _monitoringJobExecutor;

        private static void Main(string[] args)
        {
            if (!ProgramSettings.SettingsLoaded()) return;

            var actionList = new List<Action>();
            foreach (var setting in ProgramSettings.Settings.EmailSettingsList)
            {
                var monitoringJob = new MonitoringJob(setting);
                actionList.Add(monitoringJob.StartMonitoring);
            }

            _monitoringJobExecutor = new MonitoringJobExecutor(actionList);
            _monitoringJobExecutor.Start();

            while (_monitoringJobExecutor.IsRunning)
            {
                //прекращение работы по нажатию любой клавиши в консоли
                if (Console.ReadKey() != default)
                    _monitoringJobExecutor.Stop();
            }
        }

        private static void Monitor(object obj)
        {
            Console.WriteLine("hi!");
        }
    }
}
