using SmartServePOS.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	public partial class LoginView : Page
	{
		public LoginView(LoginViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
		}

		private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (DataContext is LoginViewModel vm &&
				sender is PasswordBox pb)
			{
				vm.Pin = pb.Password;
			}
		}
		private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (DataContext is LoginViewModel vm &&
					vm.LoginCommand.CanExecute(null))
				{
					vm.LoginCommand.Execute(null);
					e.Handled = true;
				}
			}
		}

	}
}
