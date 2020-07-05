using System;

namespace MailMonitor.ActionQueue
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