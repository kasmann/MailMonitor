using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailMonitor
{
    public class MonitoringJobExecutor : IDisposable
    {
        public bool IsRunning { get; private set; }
        private readonly object _locker = new object();
        private Task _task;
        private readonly Queue<Action> _queue = new Queue<Action>();

        public MonitoringJobExecutor(IEnumerable<Action> actionList)
        {
            foreach (var action in actionList)
            {
                Add(action);
            }
        }

        private void Add(Action action)
        {
            if (action == null) return;
            
            lock (_locker)
            {
                _queue.Enqueue(action);
            }
        }

        public void Start()
        {
            if (IsRunning)
            {
                throw new JobExecutorStartedException("\nМониторинг уже запущен!");
            }

            IsRunning = true;
            _task = Task.Run(Work);
            
            Console.WriteLine("\nМониторинг запущен.");
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                throw new JobExecutorStoppedException("\nМониторинг остановлен или не был начат.");
            }

            IsRunning = false;
            _task.Wait(); 
                        
            Console.WriteLine("\nМониторинг остановлен.");
        }

        public void Dispose()
        {
            if (IsRunning) Stop();
            _task.Dispose();
        }        
        
        private void Work()
        {
            while (IsRunning)
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
                            try
                            {
                                action.Invoke();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Возникла ошибка при исполнении метода {action.Method}.\n{ex.Message}\n{ex}");
                            }
                        });
                }
            }
        }
    }
}