using System;
using System.Collections.Generic;

namespace MailMonitor
{
    public class ActionQueue
    {
        private readonly Queue<Action> _queue = new Queue<Action>();
        private readonly object _locker = new object();
        public int Count { get => _queue.Count; }

        public void Add(Action action)
        {
            if (action == null) return;

            lock (_locker)
            {
                _queue.Enqueue(action);
            }
        }

        public Action Dequeue()
        {
            Action action = null;

            if (_queue.Count > 0)
            {
                lock (_locker)
                {
                    action = _queue.Dequeue();
                }
            }

            return action;
        }
    }
}
