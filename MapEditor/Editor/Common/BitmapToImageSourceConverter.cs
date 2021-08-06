using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Editor.Common
{
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public static BitmapImage Convert(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
             Convert(value as Bitmap);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            null;
    }
}
