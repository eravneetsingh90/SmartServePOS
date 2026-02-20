using SmartServe.Domain.Services;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class LoginViewModel : BaseViewModel
	{
		private readonly MainWindowViewModel _mainWindowVm;

		private readonly IAuthService _authService;
		private readonly INavigationService _navigationService;

		private string _username = "user";
		private string _pin = "1234";
		private string _errorMessage = string.Empty;
		private bool _isBusy;

		public event Action? LoginSucceeded;
		public ICommand LoginCommand { get; }

		public LoginViewModel(IAuthService authService, INavigationService navigationService, MainWindowViewModel mainWindowVm)
		{
			_mainWindowVm = mainWindowVm;
			_authService = authService;
			_navigationService = navigationService;
			LoginCommand = new RelayCommand(async _ => await LoginAsync());
		}

		public string Username
		{
			get => _username;
			set
			{
				_username = value;
				OnPropertyChanged();
			}
		}

		public string Pin
		{
			private get => _pin;
			set => _pin = value;
		}

		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				OnPropertyChanged();
			}
		}

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				_isBusy = value;
				OnPropertyChanged();
			}
		}

		public async Task LoginAsync()
		{
			if (IsBusy)
				return;

			ErrorMessage = string.Empty;

			if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Pin))
			{
				ErrorMessage = "Please enter username and PIN";
				return;
			}

			try
			{
				IsBusy = true;

				UserEntity? user = await _authService.LoginAsync(Username, Pin);

				if (user == null)
				{
					ErrorMessage = "Invalid username or PIN";
					Pin = string.Empty;
					return;
				}

				// Success
				_mainWindowVm.IsLoggedIn = true;
				_mainWindowVm.IsAdmin = user.Role?.RoleName == "OWNER" ? true : false;
				_navigationService.NavigateToTableView();
			}
			catch
			{
				ErrorMessage = "Something went wrong. Please try again.";
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}

