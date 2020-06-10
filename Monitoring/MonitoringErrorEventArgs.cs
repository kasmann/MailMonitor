using System;

namespace MailMonitor
{
	public class MonitoringErrorEventArgs : EventArgs
	{
		public string Message;
		public string Method;
		public string ErrorMessage;
	}
}
