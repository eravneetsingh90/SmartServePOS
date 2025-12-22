using System.Windows.Input;

namespace SmartServePOS.Command
{
	public class RelayCommand<T> : ICommand
	{
		private readonly Action<T?> _execute;
		private readonly Func<T?, bool>? _canExecute;

		public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
		public void Execute(object? parameter) => _execute((T?)parameter);
		public event EventHandler? CanExecuteChanged;
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	public class RelayCommand : ICommand
	{
		private readonly Action<object?> _execute;
		private readonly Predicate<object?>? _canExecute;

		public RelayCommand(Action<object?> execute,
							Predicate<object?>? canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object? parameter)
			=> _canExecute == null || _canExecute(parameter);

		public void Execute(object? parameter)
			=> _execute(parameter);

		public event EventHandler? CanExecuteChanged;
	}

}
