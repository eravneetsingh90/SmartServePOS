using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartServePOS.Converters
{
	public class BoolToGridLengthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isLoggedIn = (bool)value;
			return isLoggedIn ? new GridLength(50) : new GridLength(0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}

}
