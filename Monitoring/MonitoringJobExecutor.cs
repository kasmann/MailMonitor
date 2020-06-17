using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace MailMonitor
{
    public class MonitoringJobExecutor : IDisposable
    {
        public bool IsRunning { get; private set; }
        public event EventHandler<MonitoringErrorEventArgs> OnErrorOccured;

        private readonly object _locker = new object();
        private readonly IActionQueue _queue;
        private EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
        private Semaphore _semaphore;
        private int _maxConcurrent;
        private readonly ILogger _logger;

        public MonitoringJobExecutor(IActionQueue queue, ILogger logger)
        {
            _logger = logger;
            _queue = queue;
            
            //не придумала, как безопасно обработать уже существующие в очереди задачи,
            //если они были добавлены до старта обработчика и подписки на событие добавления задачи,
            //поэтому очистка очереди, если она была непустой
            if (_queue.Count > 0 ) _queue.Clear();
            
            _queue.OnActionAdded += Work;
        }

        public void Start(int maxConcurrent)
        {
            if (IsRunning)
            {
                throw new JobExecutorStartedException("Мониторинг уже запущен!\n");
            }

            IsRunning = true;
            _maxConcurrent = maxConcurrent;
            _semaphore = new Semaphore(_maxConcurrent, _maxConcurrent);

            _logger.LogInformation("Мониторинг запущен.\n");
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                throw new JobExecutorStoppedException("Мониторинг уже остановлен или не был начат.\n");
            }

            IsRunning = false;

            _logger.LogInformation("Мониторинг остановлен.\n");
        }

        public void Dispose()
        {
            if (IsRunning) Stop();
            _queue.OnActionAdded -= Work;
        }

        private void Work(object obj, EventArgs args)
        {
            Action action = null;

            lock (_locker)
            {
                if (_queue.Count > 0)
                {
                    action = _queue.Dequeue();
                }
            }

            if (action != null)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(
                        _ =>
                        {
                            _semaphore.WaitOne();
                            try
                            {
                                action.Invoke();
                            }
                            catch (Exception ex)
                            {
                                MonitoringErrorEventArgs margs = new MonitoringErrorEventArgs
                                {
                                    Message = "Возникла ошибка при исполнении метода",
                                    Method = action.Method.Name,
                                    ErrorMessage = ex.Message
                                };
                                OnErrorOccured?.Invoke(this, margs);
                            }
                            finally
                            {
                                _semaphore.Release();
                            }
                        });
                }
                catch(Exception ex)
                {
                    MonitoringErrorEventArgs margs = new MonitoringErrorEventArgs
                    {
                        Message = "Не удалось поместить метод в очередь выполнения.",
                        Method = action.Method.Name,
                        ErrorMessage = ex.Message
                    };
                    OnErrorOccured?.Invoke(this, margs);
                }
            }
        }
    }
}