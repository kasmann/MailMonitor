using System;
using System.Collections.Generic;

namespace MailMonitor
{
    public class ActionQueue : Queue<Action>, IActionQueue
    {
        public event EventHandler OnActionAdded;
        
        private readonly object _locker = new object();

        public new void Enqueue(Action action)
        {
            if (action == null) return;
            
            lock (_locker)
            {
                base.Enqueue(action);
                OnActionAdded?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}