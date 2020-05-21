using System;

namespace MailMonitor
{
    public partial class MonitoringJob
    {
        private static class MessageActions
        {
            public static void Print(string login)
            {
                Console.WriteLine($"Учетная запись {login}. Получено 1 новое сообщение. Письмо отправлено на печать.");
            }

            public static void Forward(string login, string forwardTo)
            {
                Console.WriteLine($"Учетная запись {login}. Получено 1 новое письмо. Письмо перенаправлено адресату {forwardTo}.");
            }

            public static void CopyTo(string login, string path)
            {
                Console.WriteLine($"Учетная запись {login}. Получено 1 новое письмо. Письмо скопировано в локальную папку {path}.");
            }

            public static void Notify(string login)
            {
                Console.WriteLine($"Учетная запись {login}. Получено 1 новое сообщение. Оповещение.");
            }
        }
    }
}
