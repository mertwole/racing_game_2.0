using System.Windows;

namespace Editor
{
    public class RequestModelEventArgs : RoutedEventArgs
    {
        IViewModel requester;
        public IViewModel Requester { get => requester; }

        public RequestModelEventArgs(object source, IViewModel vm)
        {
            RoutedEvent = RequestModel.RequestModelEvent;
            Source = source;
            requester = vm;
        }
    }

    public class RequestModel
    {
        public static readonly RoutedEvent RequestModelEvent = EventManager.RegisterRoutedEvent(
            "RequestModel", RoutingStrategy.Bubble, typeof(RequestModelEventArgs), typeof(RequestModel));

        public static void AddRequestModelHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
        {
            if (!(dependencyObject is UIElement uiElement))
                return;

            uiElement.AddHandler(RequestModelEvent, handler);
        }

        public static void RemoveRequestModelHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
        {
            if (!(dependencyObject is UIElement uiElement))
                return;

            uiElement.RemoveHandler(RequestModelEvent, handler);
        }
    }
}
