using SmartServe.EFCore.Models;
using SmartServePOS.Helper;
using SmartServePOS.ViewModels;

namespace SmartServePOS.Models
{
	public class ProductVariantModel : BaseViewModel
	{
		public int ProductVariantId { get; set; }

		public int ProductId { get; set; }

		public int? BrandId { get; set; }

		public string Name { get; set; } = null!;

		public decimal Price { get; set; }

		public bool? IsActive { get; set; }

		public DateTime? CreatedAt { get; set; }

		public int DisplayOrder { get; set; }
		private StockMode _stockMode;
		public StockMode StockMode
		{
			get => _stockMode;
			set
			{
				if (_stockMode != value)
				{
					_stockMode = value;
					OnPropertyChanged();
				}
			}
		}
		
		//public virtual Brand? Brand { get; set; }

		//public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

		//public virtual Product Product { get; set; } = null!;

		//public virtual ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();

		//public virtual Stock? Stock { get; set; }

		//public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
	}
}
