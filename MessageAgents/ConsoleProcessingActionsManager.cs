using Microsoft.Extensions.Logging;

namespace MailMonitor.MessageAgents
{
    public class ConsoleProcessingActionsManager : IProcessingActionsManager
    {
        private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;

        public ConsoleProcessingActionsManager()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _loggerFactory.CreateLogger<Program.Program>();
        }

        public void Print(string login)
        {
            _logger.LogInformation($"Учетная запись {login}. Получено 1 новое сообщение. Письмо отправлено на печать.");
        }

        public void Forward(string login, string forwardTo)
        {
            _logger.LogInformation(
                $"Учетная запись {login}. Получено 1 новое письмо. Письмо перенаправлено адресату {forwardTo}.");
        }

        public void CopyTo(string login, string path)
        {
            _logger.LogInformation(
                $"Учетная запись {login}. Получено 1 новое письмо. Письмо скопировано в локальную папку {path}.");
        }

        public void Notify(string login)
        {
            _logger.LogInformation($"Учетная запись {login}. Получено 1 новое сообщение. Оповещение.");
        }
    }
}
