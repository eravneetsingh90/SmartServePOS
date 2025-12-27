using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SmartServePOS.ViewModels
{
	public class TableViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<GetTableViewDto> Tables { get; } = new();
		private readonly INavigationService _navigationService;
		public ICommand OpenTableCommand { get; }
		private readonly IRestaurantTableStore _tableStore;
		public TableViewModel(
			IRestaurantTableStore tableStore, 
			INavigationService navigationService)
		{
			_tableStore = tableStore ?? throw new ArgumentNullException(nameof(tableStore));
			_navigationService = navigationService;
			OpenTableCommand = new RelayCommand<GetTableViewDto>(OpenTable);

			_ = InitializeAsync();
		}

		private async Task InitializeAsync()
		{
			try
			{
				var dtos = await _tableStore.GetTablesForViewAsync();
				Tables.Clear();
				foreach (var d in dtos)
				{
					Tables.Add(new GetTableViewDto
					{
						TableId = d.TableId,
						DisplayName = d.DisplayName ?? string.Empty,
						OrderId = d.OrderId,
						StatusName = d.StatusName,
						ColorHex = d.ColorHex,
						Amount = d.Amount,
						//IsOccupied = d.OrderId != null
					});
				}
				Notify(nameof(Tables));
			}
			catch (Exception)
			{
				// swallow or log as appropriate; leave sample fallback if desired.
			}
		}

		private async void OpenTable(GetTableViewDto table)
		{
			if (table == null)
				return;

			_navigationService.NavigateToBilling(table.OrderId??0);
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void Notify([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

}
