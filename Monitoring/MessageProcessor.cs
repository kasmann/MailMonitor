using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MailMonitor
{
    public partial class MonitoringJob
    {
        private class MessageProcessor
        {
            private readonly string _login;
            private readonly MailMessage _message;
            private readonly IEnumerable<MonitoringSettings> _monitoringSettingsList;
            private readonly IProcessingActionsManager _actionsManager;

            public MessageProcessor(string login, MailMessage message, IEnumerable<MonitoringSettings> monitoringSettingsList, IProcessingActionsManager actionsManager)
            {
                _login = login;
                _message = message;
                _monitoringSettingsList = monitoringSettingsList;
                _actionsManager = actionsManager;
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
                  
                    
                    if (monitoringSetting.Notify) _actionsManager.Notify(_login);
                    if (monitoringSetting.CopyTo) _actionsManager.CopyTo(_login, @"d:\incoming\");
                    if (monitoringSetting.Forward) _actionsManager.Forward(_login, monitoringSetting.ForwardTo);
                    if (monitoringSetting.Print) _actionsManager.Print(_login);
                }
            }            
        }
    }
}
