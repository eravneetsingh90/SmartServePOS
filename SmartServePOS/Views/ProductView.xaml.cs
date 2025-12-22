using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
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

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for ProductView.xaml
	/// </summary>
	public partial class ProductView : Page
	{
		public ProductView()
		{
			InitializeComponent();
			if (DataContext == null)
			{
				if (App.Services is not null)
				{
					DataContext = App.Services.GetService<ProductViewModel>();
				}
			}
		}
	}
}
