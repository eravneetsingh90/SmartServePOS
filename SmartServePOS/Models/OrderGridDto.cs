using SmartServePOS.ViewModels;
using System.Collections.ObjectModel;

namespace SmartServePOS.Models
{
	public class OrderGridDto : BaseViewModel
	{
		public int Id { get; set; }
		public string OrderNumber { get; set; }
		public string DisplayTime { get; set; } // IST formatted
		public string OrderType { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; }

		// 🔽 Expand / Collapse
		private bool _isExpanded;
		public bool IsExpanded
		{
			get => _isExpanded;
			set { _isExpanded = value; OnPropertyChanged(); }
		}

		// 🔹 Loaded on expand (lazy)
		public ObservableCollection<OrderItemDto> Items { get; }
		= new ObservableCollection<OrderItemDto>();

		public decimal Discount { get; set; }
	}

}
