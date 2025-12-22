using System.Globalization;
using System.Windows.Data;

namespace SmartServePOS.Converters
{
	public class BoolToHitTestConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// When searching → disable interaction
			return value is bool isActive ? !isActive : true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
