using HandyControl.Controls;

namespace SmartServePOS.Helper
{
	public class HandyNotificationService : INotificationService
	{
		public void Success(string message) => Growl.Success(message);
		public void Warning(string message) => Growl.Warning(message);
		public void Error(string message) => Growl.Error(message);
		public void Info(string message) => Growl.Info(message);
	}

}
