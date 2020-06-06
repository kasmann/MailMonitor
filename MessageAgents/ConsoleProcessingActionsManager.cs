using System;

namespace MailMonitor
{
    public class ConsoleProcessingActionsManager : IProcessingActionsManager
    {
        public void Print(string login)
        {
            Console.WriteLine($"Учетная запись {login}. Получено 1 новое сообщение. Письмо отправлено на печать.");
        }

        public void Forward(string login, string forwardTo)
        {
            Console.WriteLine(
                $"Учетная запись {login}. Получено 1 новое письмо. Письмо перенаправлено адресату {forwardTo}.");
        }

        public void CopyTo(string login, string path)
        {
            Console.WriteLine(
                $"Учетная запись {login}. Получено 1 новое письмо. Письмо скопировано в локальную папку {path}.");
        }

        public void Notify(string login)
        {
            Console.WriteLine($"Учетная запись {login}. Получено 1 новое сообщение. Оповещение.");
        }
    }
}
