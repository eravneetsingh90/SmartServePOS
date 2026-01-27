using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Views;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class MainWindowViewModel : BaseViewModel
	{
		private readonly INavigationService _navigationService;
		public ICommand LogoutCommand { get; }
		private bool _isLoggedIn;

		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set
			{
				_isLoggedIn = value;
				OnPropertyChanged();
			}
		}

		private bool _isAdmin;

		public bool IsAdmin
		{
			get => _isAdmin;
			set
			{
				_isAdmin = value;
				OnPropertyChanged();
			}
		}
		private bool _isLoading = true;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				OnPropertyChanged();
			}
		}
		public MainWindowViewModel(INavigationService navigationService)
		{
			_navigationService = navigationService;
			LogoutCommand = new RelayCommand(Logout);
		}
		private void Logout(object? _)
		{
			// 1️⃣ Clear session
			//AuthSession.Logout();

			// 2️⃣ Reset POS runtime
			//PosState.Reset();

			// 3️⃣ Update UI state
			IsLoggedIn = false;

			// 4️⃣ Navigate to Login
			_navigationService.NavigateTo<LoginView>();
		}
	}

}
