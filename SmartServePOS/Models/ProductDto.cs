using SmartServePOS.ViewModels;
using System.Windows.Media;

namespace SmartServePOS.Models
{
	public class ProductDto : BaseViewModel
	{
		private string? _foodType;

		public int Id { get; set; }
		public int LocalId { get; set; }
		public string Name { get; set; }

		public string? FoodType
		{
			get => _foodType;
			set
			{
				_foodType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FoodTypeColor));
			}
		}

		public Brush FoodTypeColor
		{
			get
			{
				return FoodType switch
				{
					"VEG" => Brushes.Green,
					"NON_VEG" => Brushes.Red,
					"EGG" => Brushes.Goldenrod,
					_ => Brushes.Transparent
				};
			}
		}

		public int? CategoryId { get; set; }

		public bool IsActive { get; set; }

		public int DisplayOrder { get; set; }
		public DateTime UpdatedOn { get; set; }

	}

}
