using SmartServePOS.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class TableViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<TableItem> Tables { get; } = new();

		// Simple command placeholder - replace with your navigation/logic
		public ICommand OpenTableCommand { get; }

		public TableViewModel()
		{
			// Sample data - produce a grid of tables for demo
			for (int i = 1; i <= 24; i++)
			{
				Tables.Add(new TableItem { Name = $"Table {i}", IsOccupied = (i % 3 == 0) });
			}

			OpenTableCommand = new RelayCommand<TableItem>(t =>
			{
				// TODO: open table details / navigate
			});
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void Notify([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public class TableItem
	{
		public string Name { get; set; } = string.Empty;
		public bool IsOccupied { get; set; }
		public string Status => IsOccupied ? "Occupied" : "Available";
	}
}
