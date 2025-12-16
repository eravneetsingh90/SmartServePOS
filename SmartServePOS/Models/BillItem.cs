using System.ComponentModel;

namespace SmartServePOS.Models
{
	public class BillItem : INotifyPropertyChanged
	{
		public string ItemName { get; set; }
		public decimal UnitPrice { get; set; }

		private int _quantity = 1;
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

		public decimal TotalPrice => UnitPrice * Quantity;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
