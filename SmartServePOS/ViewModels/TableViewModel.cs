using SmartServe.Domain.Models;
using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using SmartServePOS.Models;
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
		public ObservableCollection<GetTableViewDto> Tables { get; } = new();

		// Simple command placeholder - replace with your navigation/logic
		public ICommand OpenTableCommand { get; }

		private readonly RestaurantTableStore _tableStore;

		public TableViewModel(RestaurantTableStore tableStore)
		{
			_tableStore = tableStore ?? throw new ArgumentNullException(nameof(tableStore));

			OpenTableCommand = new RelayCommand<GetTableViewDto>(t =>
			{
				// TODO: open table details / navigate
			});

			// start loading tables asynchronously (fire-and-forget)
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

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void Notify([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

}
