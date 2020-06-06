using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using S22.Imap;

namespace MailMonitor
{
    public partial class MonitoringJob : IDisposable
    {
        private ImapClient _imapClient;
        private readonly string _login;
        private readonly string _password;
        private readonly string _server;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly int _timeout;
        private readonly List<MonitoringSettings> _monitoringSettingsList;
        private readonly List<Task> _messageProcessingTasks;
        private readonly IProcessingActionsManager _actionsManager;
        private CancellationTokenSource _cts;

        public MonitoringJob(EmailSettings emailSettings, IProcessingActionsManager actionsManager)
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
            _actionsManager = actionsManager;
        }

        public void StartMonitoring()
        {
            if (ImapClientCreated()) Console.WriteLine($"Учетная запись {_login}. Клиент IMAP4 создан. Подключение успешно.\n");
            else return;
           
            if (ImapClientLoggedIn()) Console.WriteLine($"Учетная запись {_login}. Авторизация прошла успешно.\n");

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
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка сокета {ex.SocketErrorCode}.\n{ex.Message}");
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
            _cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(Monitor, _cts.Token);
        }

        private void Monitor(object obj)
        {
            var token = (CancellationToken) obj;
                
            while(!token.IsCancellationRequested)
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
                var messageProcessor = new MessageProcessor(_login, message, _monitoringSettingsList, _actionsManager);
                messageProcessor.StartProcessing();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts?.Dispose();

            if (_messageProcessingTasks.Count > 0)
            {
                Task.WaitAll(_messageProcessingTasks.ToArray());
            }

            _imapClient.Logout();
            _imapClient?.Dispose();
        }
    }
}
