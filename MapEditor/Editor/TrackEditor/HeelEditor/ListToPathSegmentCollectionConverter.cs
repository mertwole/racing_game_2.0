using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Editor.TrackEditor.HeelEditor
{
    public class ListToPathSegmentCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as List<LineSegment>;
            PathSegmentCollection collection = new PathSegmentCollection(list.Count);
            foreach (var seg in list)
                collection.Add(seg);
            return collection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
