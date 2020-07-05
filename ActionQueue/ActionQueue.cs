using System;
using System.Collections.Concurrent;

namespace MailMonitor.ActionQueue
{
    public class ActionQueue : ConcurrentQueue<Action>, IActionQueue
    {
        public event EventHandler OnActionAdded;

        void IActionQueue.Enqueue(Action action)
        {
            if (action == null) return;
            
            base.Enqueue(action);
            OnActionAdded?.Invoke(this, EventArgs.Empty);
        }

        Action IActionQueue.Dequeue()
        {
            return !TryDequeue(out var result) ? null : result;
        }
    }
}