using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public interface IPOSBillingService
	{
		Task<List<GetTableViewDto>> GetTablesForViewAsync();

	}
}
