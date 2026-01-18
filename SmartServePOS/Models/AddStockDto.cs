using SmartServe.Domain.Constants;
using SmartServePOS.Helper;
using SmartServePOS.ViewModels;

namespace SmartServePOS.Models
{
	public class AddStockModel : BaseViewModel
	{
		public string ItemType { get; set; }

		public int VariantId { get; set; }

		public string DisplayName { get; set; }

		public string Unit { get; set; }

		private decimal _quantity = 1;
		public decimal Quantity
		{
			get => _quantity;
			set => SetProperty(ref _quantity, value);
		}

		private string _reason = "PURCHASE";
		public string Reason
		{
			get => _reason;
			set => SetProperty(ref _reason, value);
		}
		public string SearchText { get; init; } = string.Empty;
	}
}
