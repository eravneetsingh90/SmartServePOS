using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	public partial class LoginView : Window
	{
		private readonly LoginViewModel _viewModel;

		public LoginView(LoginViewModel viewModel)
		{
			InitializeComponent();

			_viewModel = viewModel;
			DataContext = _viewModel;

			// Close window when login succeeds
			_viewModel.LoginSucceeded += OnLoginSucceeded;
		}

		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (sender is PasswordBox passwordBox)
			{
				_viewModel.Pin = passwordBox.Password;
			}
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.LoginAsync();
		}

		private async void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				await _viewModel.LoginAsync();
			}
		}

		private void OnLoginSucceeded()
		{
			// Close login window
			this.Close();

			// Show main window and navigate to POS page inside it
			var mainWindow = App.Services.GetRequiredService<MainWindow>();
			mainWindow.Show();

			var posPage = App.Services.GetRequiredService<POSView>();
			// POSView is a Window, not a Page, so just navigate directly
			mainWindow.Navigate(posPage);
		}
	}
}
