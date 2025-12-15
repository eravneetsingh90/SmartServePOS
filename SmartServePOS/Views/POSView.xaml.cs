using System.Windows;
using System.Windows.Controls;
using SmartServePOS.ViewModels;

namespace SmartServePOS.Views
{
	public partial class POSView : Page
	{
		public POSView()
		{
			InitializeComponent();

			// Don't overwrite an externally-provided DataContext (DI) - only set a default for design/runtime.
			if (DataContext == null)
			{
				DataContext = new POSViewModel();
			}
		}

		// Called by other parts of the app (for example after login) to load pages or user controls into the main area.
		public void ShowContent(UIElement content)
		{
			ContentRegion.Content = content;
		}
	}
}
