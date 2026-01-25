using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public interface IOrderSyncService
	{
		Task SyncPendingOrdersAsync();
	}
}
