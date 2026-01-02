using SmartServePOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartServePOS.Models
{
	public class BillItemModelDto : BaseViewModel
	{
		public int VariantId { get; set; }
		public string ItemName { get; set; }
		public decimal PriceSnapshot { get; set; }

		private int _quantity;
		public int Quantity
		{
			get => _quantity;
			set
			{
				_quantity = value;
				OnPropertyChanged(nameof(Quantity));
				OnPropertyChanged(nameof(TotalPrice));
			}
		}

		public decimal DiscountAmount { get; set; }   // ₹ value

		public decimal GrossPrice => Quantity * PriceSnapshot;

		public decimal TotalPrice => GrossPrice - DiscountAmount;
	}




}
