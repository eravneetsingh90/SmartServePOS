using System.Windows.Input;

namespace SmartServePOS.Helper
{
	public class AsyncRelayCommand : ICommand
	{
		private readonly Func<Task> _execute;

		public AsyncRelayCommand(Func<Task> execute)
		{
			_execute = execute;
		}

		public async void Execute(object parameter)
		{
			await _execute();
		}

		public bool CanExecute(object parameter) => true;
		public event EventHandler CanExecuteChanged;
	}

}
