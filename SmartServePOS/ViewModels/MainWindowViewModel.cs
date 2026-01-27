namespace SmartServePOS.ViewModels
{
	public class MainWindowViewModel : BaseViewModel
	{
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

	}

}
