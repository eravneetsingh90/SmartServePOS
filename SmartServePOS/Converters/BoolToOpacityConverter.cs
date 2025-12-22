using System.Globalization;
using System.Windows.Data;

namespace SmartServePOS.Converters
{
	public class BoolToOpacityConverter : IValueConverter
	{
		public double ActiveOpacity { get; set; } = 0.35;
		public double NormalOpacity { get; set; } = 1.0;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool isActive && isActive
				? ActiveOpacity
				: NormalOpacity;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
