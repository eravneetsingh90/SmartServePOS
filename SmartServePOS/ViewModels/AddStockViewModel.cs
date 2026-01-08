using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class AddStockViewModel : BaseViewModel
	{
		private readonly IMapper _mapper;
		private readonly ICatalogService _catalogService;
		private readonly IStockService _stockService;

		public ObservableCollection<BrandDto> Brands { get; } = new();
		public ObservableCollection<ProductVariantDto> Variants { get; } = new();
		public ObservableCollection<IngredientDto> Ingredients { get; } = new();

		public ObservableCollection<AddStockModel> StockRows { get; } = new();

		#region Mode

		private Helper.StockItemType _selectedMode = Helper.StockItemType.VARIANT;
		public Helper.StockItemType SelectedMode
		{
			get => _selectedMode;
			set
			{
				if (SetProperty(ref _selectedMode, value))
				{
					LoadModeData();
				}
			}
		}

		#endregion

		public ICommand AddVariantCommand { get; }
		public ICommand AddIngredientCommand { get; }
		public ICommand RemoveRowCommand { get; }
		public ICommand SaveStockCommand { get; }

		public AddStockViewModel(
			IMapper mapper,
			ICatalogService catalogService,
			IStockService stockService)
		{
			_mapper = mapper;
			_catalogService = catalogService;
			_stockService = stockService;

			AddVariantCommand = new RelayCommand<ProductVariantDto>(AddVariant);
			AddIngredientCommand = new RelayCommand<IngredientDto>(AddIngredient);
			RemoveRowCommand = new RelayCommand<AddStockModel>(r => StockRows.Remove(r));
			SaveStockCommand = new RelayCommand(async _ => await SaveStockAsync());

			LoadInitialData();
		}

		private void LoadInitialData()
		{
			foreach (var brand in _catalogService.GetBrands())
				Brands.Add(brand);
		}

		private void LoadModeData()
		{
			StockRows.Clear();
		}

		#region Add Row

		private void AddVariant(ProductVariantDto variant)
		{
			if (StockRows.Any(x =>
				x.ItemType == StockItemType.VARIANT &&
				x.ReferenceId == variant.VariantId))
				return;

			StockRows.Add(new AddStockModel
			{
				ItemType = StockItemType.VARIANT,
				ReferenceId = variant.VariantId,
				DisplayName = $"{variant.VariantName} ({variant.Brand.Name})",
				Unit = "LTR"
			});
		}

		private void AddIngredient(IngredientDto ing)
		{
			if (StockRows.Any(x =>
				x.ItemType == StockItemType.INGREDIENT &&
				x.ReferenceId == ing.IngredientId))
				return;

			StockRows.Add(new AddStockModel
			{
				ItemType = StockItemType.INGREDIENT,
				ReferenceId = ing.IngredientId,
				DisplayName = ing.Name,
				Unit = ing.Unit
			});
		}

		#endregion

		#region Save

		private async Task SaveStockAsync()
		{
			if (!StockRows.Any())
				return;

			foreach (var row in StockRows)
			{
				if (row.Quantity <= 0)
					throw new InvalidOperationException("Quantity must be greater than zero.");
			}
			var stocks = _mapper.Map<List<AddStockDto>>(StockRows.ToList());
			await _stockService.AddStockAsync(stocks);

			StockRows.Clear();
		}

		#endregion
	}
}
