using System;

namespace MailMonitor.Monitoring
{
	public class MonitoringErrorEventArgs : EventArgs
	{
		public string Message;
		public string Method;
		public string ErrorMessage;
	}
}
