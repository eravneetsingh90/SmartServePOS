using System.Windows;

namespace SmartServePOS.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		// Called by other parts of the app (for example after login) to load pages or user controls into the main area.
		public void ShowContent(UIElement content)
		{
			ContentRegion.Content = content;
		}
	}
}
