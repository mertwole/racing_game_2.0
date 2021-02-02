using System;
using System.Globalization;
using System.Windows.Data;

namespace Editor.FileManager
{
    public class CallCheckSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IContent).CheckSelected();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
