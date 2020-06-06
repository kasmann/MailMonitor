using System;
using System.Collections.Generic;

namespace MailMonitor
{
    public interface IActionQueue
    {
        event EventHandler OnActionAdded;
        int Count { get; }
        void Enqueue(Action action);
        Action Dequeue();
        void Clear();
    }
}