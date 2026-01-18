using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Constant;
using SmartServePOS.Helper;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class CategoryViewModel : BaseViewModel
	{
		private readonly IMapper _mapper;
		private readonly IProductService _productService;
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		public ObservableCollection<Category> Categories { get; } = new();

		public ICommand AddCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand RefreshCommand { get; }
		public ICommand DeleteCommand { get; }

		public CategoryViewModel(
			IMapper mapper,
			IProductService productService,
			ICategoryStore categoryStore, 
			INotificationService notificationService, 
			IDialogService dialogService)
		{
			_mapper = mapper;
			_productService = productService;
			_notificationService = notificationService;
			_dialogService = dialogService;
			AddCommand = new RelayCommand(_ => AddCategory());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			RefreshCommand = new RelayCommand(async _ => await LoadAsync());
			DeleteCommand = new RelayCommand(DeleteCategory);
		}
		public async Task Initialize() 
		{
			await LoadAsync();
		}
		private async Task LoadAsync()
		{
			Categories.Clear();
			var data = await _productService.GetCategoriesAsync();
			foreach (var c in data)
				Categories.Add(c);
		}

		private void AddCategory()
		{
			int nextOrder = Categories.Any()
			? Categories.Max(c => c.DisplayOrder) + 1
			: 1;

			Categories.Add(new Category
			{
				Name = "New Category",
				IsActive = true,
				DisplayOrder = nextOrder
			});
		}

		private async Task SaveAsync()
		{
			var response = await _productService.SaveBulkCategoriesAsync(Categories);
			if (response.MetaData.ResultCode == ResultCodes.Success)
				_notificationService.Success(UIConstants.SavedSuccessfully);
			else if (response.MetaData.ResultCode == ResultCodes.DuplicateNotAllowed)
				_notificationService.Warning(response.MetaData.ResultMessage);
			else
				_notificationService.Error(UIConstants.Error);
			await LoadAsync();
		}

		private async void DeleteCategory(object? parameter)
		{
			if (parameter is not Category category)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete category \"{category.Name}\"?");

			if (!result)
				return;

			if (category.Id != 0)
			{
				var response = await _productService.DeleteCategoryAsync(category.Id);
				if (response.MetaData.ResultCode == ResultCodes.Success)
				{
					Categories.Remove(category);
					_notificationService.Success(UIConstants.DeletedSuccessfully);
				}
				else
					_notificationService.Error(UIConstants.Error);
			}
		}
	}
}
