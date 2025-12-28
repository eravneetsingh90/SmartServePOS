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
	}

}
