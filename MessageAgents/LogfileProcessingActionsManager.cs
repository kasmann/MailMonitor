using System;
using System.IO;
using System.Net;

namespace MailMonitor
{
    public class LogfileProcessingActionsManager : IProcessingActionsManager
    {
        private readonly string _logFilePath;

        public LogfileProcessingActionsManager(string logFilePath)
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    File.AppendAllText(logFilePath, "");
                }
                catch (DirectoryNotFoundException)
                {
                    throw new DirectoryNotFoundException("Папка, содержащая лог-файл, не найдена.\nЗавершение работы.");
                }
                catch (IOException ex)
                {
                    throw new IOException(
                        $"Возникла ошибка при попытке чтения/записи.\n{ex.Message}\nЗавершение работы.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new UnauthorizedAccessException(
                        $"Ошибка доступа к лог-файлу.\n{ex.Message}\nЗавершение работы.");
                }
                catch (NotSupportedException)
                {
                    throw new NotSupportedException("Путь к лог-файлу имеет недопустимый формат.\nЗавершение работы.");
                }
            }

            _logFilePath = logFilePath;
        }

        public void Print(string login)
        {
            try
            {
                File.AppendAllText(_logFilePath,
                    $"{DateTime.Now}\tУчетная запись {login}. Получено 1 новое сообщение. Письмо отправлено на печать.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при записи в лог-файл.\n{ex.Message}");
            }
        }

        public void Forward(string login, string forwardTo)
        {
            try
            {
                File.AppendAllText(_logFilePath,
                    $"{DateTime.Now}\tУчетная запись {login}. Получено 1 новое сообщение. Письмо перенаправлено адресату {forwardTo}.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при записи в лог-файл.\n{ex.Message}");
            }
        }

        public void CopyTo(string login, string path)
        {
            try
            {
                File.AppendAllText(_logFilePath,
                    $"{DateTime.Now}\tУчетная запись {login}. Получено 1 новое сообщение. Письмо скопировано в локальную папку {path}.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при записи в лог-файл.\n{ex.Message}");
            }
        }

        public void Notify(string login)
        {
            try
            {
                File.AppendAllText(_logFilePath,
                    $"{DateTime.Now}\tУчетная запись {login}. Получено 1 новое сообщение. Оповещение.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при записи в лог-файл.\n{ex.Message}");
            }
        }
    }
}