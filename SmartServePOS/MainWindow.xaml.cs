using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartServePOS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		// Navigate to a Page instance resolved from DI
		public void Navigate(Page page)
		{
			MainFrame.Navigate(page);
		}

		// Convenience to navigate by type (DI resolution done by caller)
		public void Navigate(object content)
		{
			MainFrame.Navigate(content);
		}
	}
}
