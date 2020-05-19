using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using S22.Imap;

namespace MailMonitor
{
    public class MonitoringJob : IDisposable
    {
        private ImapClient _imapClient;
        private readonly string _login;
        private readonly string _password;
        private readonly string _server;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly int _timeout;
        private readonly List<MonitoringSettings> _monitoringSettingsList;
        private Thread _nonIdleMonitoringThread;
        private readonly List<Task> _messageProcessingTasks;

        public MonitoringJob(EmailSettings emailSettings)
        {
            if (emailSettings == null) return;
            
            _login = emailSettings.Login;
            _password = emailSettings.Password;
            _server = emailSettings.Server;
            _port = emailSettings.Port;
            _useSsl = emailSettings.UseSSL;
            _timeout = emailSettings.Timeout;
            _monitoringSettingsList = emailSettings.MonitoringSettingsList;
            _messageProcessingTasks = new List<Task>();
        }

        public void StartMonitoring()
        {
            if (ImapClientCreated()) Console.WriteLine($"\nУчетная запись {_login}. Клиент IMAP4 создан. Подключение успешно.");
            else return;
           
            if (ImapClientLoggedIn()) Console.WriteLine($"\nУчетная запись {_login}. Авторизация прошла успешно.");

            if (_imapClient.Supports("IDLE"))
            {
                StartIdling();
            }
            else
            {
                StartNonIdleMonitoring();
            }

            while (_imapClient.Authed)
            {
                //реконнект по таймауту или отправка команды NOOP
                //не поддерживается в используемой библиотеке, но нужно
                Thread.Sleep(_timeout);
            }
        }

        private bool ImapClientCreated()
        {
            try
            {
                _imapClient = new ImapClient(_server, _port, _useSsl);
            }
            catch (BadServerResponseException ex)
            {
                Console.WriteLine($"Не удалось установить соединение.\n{ex.Message}");
                return false;
            }
            return true;
        }

        private bool ImapClientLoggedIn()
        {
            try
            {
                _imapClient.Login(_login, _password, AuthMethod.Auto);
            }
            catch (InvalidCredentialsException ex)
            {
                Console.WriteLine($"Не удалось авторизоваться.\n{ex.Message}");
                return false;
            }
            return true;
        }
        
        private void StartIdling()
        {
            _imapClient.NewMessage += OnNewMessageReceived;
        }

        private void OnNewMessageReceived(object obj, IdleMessageEventArgs args)
        {
            var uids = new List<uint> {args.MessageUID};
            _messageProcessingTasks.Add(Task.Factory.StartNew(() =>
            {
                ProcessMessages(uids);
            }));
        }
        
        private void StartNonIdleMonitoring()
        { 
            _nonIdleMonitoringThread = new Thread(_ => Monitor());
            _nonIdleMonitoringThread.IsBackground = true;
            _nonIdleMonitoringThread.Start();
        }

        private void Monitor()
        {
            while (_imapClient.Authed)
            {
                var uids = _imapClient.Search(SearchCondition.Unseen()).ToList();
                if (uids.Count > 0) ProcessMessages(uids);
                Thread.Sleep(_timeout);
            }
        }

        private void ProcessMessages(IEnumerable<uint> uids)
        {
            object messages;
            try
            {
                messages = _imapClient.GetMessages(uids, FetchOptions.NoAttachments);
            }
            catch (BadServerResponseException ex)
            {
                Console.WriteLine($"Не удалось загрузить новые письма.\n{ex.Message}");
                return;
            }

            if (messages == null) return;
            
            foreach (var message in (IEnumerable<MailMessage>)messages)
            {
                var messageProcessor = new MessageProcessor(_login, message, _monitoringSettingsList);
                messageProcessor.StartProcessing();
            }
        }

        private class MessageProcessor
        {
            private readonly string _login;
            private readonly MailMessage _message;
            private readonly IEnumerable<MonitoringSettings> _monitoringSettingsList;

            public MessageProcessor(string login, MailMessage message, IEnumerable<MonitoringSettings> monitoringSettingsList)
            {
                _login = login;
                _message = message;
                _monitoringSettingsList = monitoringSettingsList;
            }

            public void StartProcessing()
            {
                foreach (var monitoringSetting in _monitoringSettingsList)
                {
                    var inspectingPart = monitoringSetting.EmailPart switch
                    {
                        "body" => _message.Body,
                        "title" => _message.Subject,
                        "from" => _message.From.Address,
                        "to" => _message.To,
                        _ => new object()
                    };

                    if (inspectingPart is string inspectingString)
                    {
                        if (!Regex.IsMatch(inspectingString, monitoringSetting.Condition)) return;
                    }
                    else if (inspectingPart is MailAddressCollection receivers)
                    {
                        if (receivers.Any(receiver =>
                            !Regex.IsMatch(receiver.Address, monitoringSetting.Condition))) return;
                    }
                    
                    if (monitoringSetting.Notify) Notify();
                    if (monitoringSetting.CopyTo) CopyTo();
                    if (monitoringSetting.Forward) Forward(monitoringSetting.ForwardTo);
                    if (monitoringSetting.Print) Print();
                }
            }

            private void Print()
            {
                Console.WriteLine($"Учетная запись {_login}. Получено 1 новое сообщение. Письмо отправлено на печать.");
            }

            private void Forward(string forwardTo)
            {
                Console.WriteLine($"Учетная запись {_login}. Получено 1 новое письмо. Письмо перенаправлено адресату {forwardTo}.");
            }

            private void CopyTo()
            {
                Console.WriteLine($"Учетная запись {_login}. Получено 1 новое письмо. Письмо скопировано в локальную папку.");
            }

            private void Notify()
            {
                Console.WriteLine($"Учетная запись {_login}. Получено 1 новое сообщение. Оповещение.");
            }
        }

        public void Dispose()
        {
            _nonIdleMonitoringThread.Abort();
            _nonIdleMonitoringThread.Join();
            
            Task.WaitAll(_messageProcessingTasks.ToArray());
            
            _imapClient.Logout();
            _imapClient.Dispose();
        }
    }
}
