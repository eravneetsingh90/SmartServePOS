using System.Windows;

namespace SmartServePOS.Helper
{
	public class HandyDialogService : IDialogService
	{
		public Task ShowInfoAsync(string title, string message)
		{
			HandyControl.Controls.MessageBox.Show(
				message,
				title,
				MessageBoxButton.OK,
				MessageBoxImage.Information);

			return Task.CompletedTask;
		}

		public Task ShowWarningAsync(string title, string message)
		{
			HandyControl.Controls.MessageBox.Show(
				message,
				title,
				MessageBoxButton.OK,
				MessageBoxImage.Warning);

			return Task.CompletedTask;
		}

		public Task ShowErrorAsync(string title, string message)
		{
			HandyControl.Controls.MessageBox.Show(
				message,
				title,
				MessageBoxButton.OK,
				MessageBoxImage.Error);

			return Task.CompletedTask;
		}

		public Task<bool> ShowConfirmAsync(string title, string message)
		{
			var result = HandyControl.Controls.MessageBox.Show(
				message,
				title,
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			return Task.FromResult(result == MessageBoxResult.Yes);
		}
	}
}
