using System.Net.NetworkInformation;

namespace SmartServePOS.Helper
{
	public static class NetworkHelper
	{
		public static bool IsInternetAvailable()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			try
			{
				using var ping = new Ping();
				var reply = ping.Send("8.8.8.8", 1000);
				return reply?.Status == IPStatus.Success;
			}
			catch
			{
				return false;
			}
		}
	}

}
