using System.Windows;
using System.Windows.Media;

namespace Editor.Common
{
    public class ChildFinder
    {
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!string.IsNullOrEmpty(childName))
                {
                    var framework_element = child as FrameworkElement;
                    if (framework_element?.Name == childName)
                        return (T)child;
                }
                else if(child is T ch)
                    return ch;

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }
    }
}
