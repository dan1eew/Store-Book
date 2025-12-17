using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Store_Book
{
    public class DiscountToBackgroundConverter : IValueConverter
    {
        public static DiscountToBackgroundConverter Instance { get; } = new DiscountToBackgroundConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal discount)
            {
                if (discount > 15) return new SolidColorBrush(Colors.DarkRed);
                if (discount > 10) return new SolidColorBrush(Colors.OrangeRed);
                if (discount > 5) return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}