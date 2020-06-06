namespace MailMonitor
{
    public interface IProcessingActionsManager
    {
        void Print(string login);

        void Forward(string login, string forwardTo);

        void CopyTo(string login, string path);

        void Notify(string login);
    }
}