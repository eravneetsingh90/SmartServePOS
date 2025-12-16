using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class BillingViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<BillItem> BillItems { get; set; }

		public ICommand IncreaseQtyCommand { get; }
		public ICommand DecreaseQtyCommand { get; }

		public BillingViewModel()
		{
			BillItems = new ObservableCollection<BillItem>();

			IncreaseQtyCommand = new RelayCommand<BillItem>(item =>
			{
				item.Quantity++;
				OnPropertyChanged(nameof(GrandTotal));
			});

			DecreaseQtyCommand = new RelayCommand<BillItem>(item =>
			{
				if (item.Quantity > 1)
					item.Quantity--;
				OnPropertyChanged(nameof(GrandTotal));
			});
		}

		public decimal GrandTotal =>
			BillItems.Sum(i => i.TotalPrice);

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
