using System;
using System.Threading;

namespace MailMonitor
{
    public class MonitoringJobExecutor : IDisposable
    {
        public bool IsRunning { get; private set; }
        private readonly object _locker = new object();
        private readonly IActionQueue _queue;
        private EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
        private Semaphore _semaphore;
        private int _maxConcurrent;

        public MonitoringJobExecutor(IActionQueue queue)
        {
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

            Console.WriteLine("Мониторинг запущен.\n");
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                throw new JobExecutorStoppedException("Мониторинг уже остановлен или не был начат.\n");
            }

            IsRunning = false;

            Console.WriteLine("Мониторинг остановлен.\n");
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
                            Console.WriteLine(
                                $"Возникла ошибка при исполнении метода {action.Method}.\n{ex.Message}\n{ex}");
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    });
            }
        }
    }
}