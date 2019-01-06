using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Converters
{
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new GridLength((double)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ((GridLength)value).Value;
    }
}
