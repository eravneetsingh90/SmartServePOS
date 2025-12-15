using SmartServe.Domain.Services;
using SmartServe.EFCore.Entities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SmartServePOS.ViewModels
{
	public class LoginViewModel : INotifyPropertyChanged
	{
		private readonly AuthService _authService;

		private string _username = "user";
		private string _pin = string.Empty;
		private string _errorMessage = string.Empty;
		private bool _isBusy;

		public event PropertyChangedEventHandler? PropertyChanged;
		public event Action? LoginSucceeded;

		public LoginViewModel(AuthService authService)
		{
			_authService = authService;
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

		// Set from PasswordBox (not bound)
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

				User? user = await _authService.LoginAsync(Username, Pin);

				if (user == null)
				{
					ErrorMessage = "Invalid username or PIN";
					Pin = string.Empty;
					return;
				}

				// Success
				LoginSucceeded?.Invoke();
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

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

