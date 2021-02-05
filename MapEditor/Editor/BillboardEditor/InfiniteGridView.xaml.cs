using Editor.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Editor.BillboardEditor
{
    public class UIElementWpapper : INotifyPropertyChanged
    {
        UIElement element;
        public UIElement Element { get => element; }

        public Point Position { get; set; }
        public Size Size { get; set; }

        public UIElementWpapper(Point position, Size size, UIElement element)
        {
            this.element = element;
            Position = position;
            Size = size;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Update()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Size"));
        }
    }

    public class WorldXToScreenSpaceConverter : IValueConverter
    {
        // Parameter is binding proxy.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data_context = (parameter as BindingProxy).Data as InfiniteGridView;
            if (data_context == null) return 0;
            return data_context.WorldToScreenSpace(new Point((double)value, 0)).X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            throw new NotImplementedException();
    }

    public class WorldYToScreenSpaceConverter : IValueConverter
    {
        // Parameter is binding proxy.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data_context = (parameter as BindingProxy).Data as InfiniteGridView;
            if (data_context == null) return 0;
            return data_context.WorldToScreenSpace(new Point(0, (double)value)).Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class WorldSizeToScreenSpaceConverter : IValueConverter
    {
        // Parameter is binding proxy.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data_context = (parameter as BindingProxy).Data as InfiniteGridView;
            if (data_context == null) return 0;
            return data_context.WorldSizeToScreenSpace((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ObservableGrid : Grid
    {
        public delegate void ChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved);
        public event ChildrenChanged OnChildrenChanged;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            OnChildrenChanged.Invoke(visualAdded, visualRemoved);
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }

    [ContentProperty("Children")]
    public partial class InfiniteGridView : UserControl
    {
        // Offset in pixels relative to center.
        Point fieldOffset = new Point(0, 0);

        double pixelsInUnit = 30;

        ObservableCollection<UIElementWpapper> childrenWrapped = new ObservableCollection<UIElementWpapper>();
        public ObservableCollection<UIElementWpapper> ChildrenWpapped { get => childrenWrapped; }

        public Point WorldToScreenSpace(Point world_pos) => 
            new Point(world_pos.X * pixelsInUnit + fieldOffset.X + GridCanvas.ActualWidth * 0.5, 
                world_pos.Y * pixelsInUnit + fieldOffset.Y + GridCanvas.ActualHeight * 0.5);

        public double WorldSizeToScreenSpace(double world_size) =>
            world_size * pixelsInUnit;

        // Children.
        public static readonly DependencyPropertyKey ChildrenProperty =
        DependencyProperty.RegisterReadOnly("Children",
        typeof(UIElementCollection), typeof(InfiniteGridView), new PropertyMetadata());
        public UIElementCollection Children {
            get => (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        #region Attached properties

        // Attached X position.
        public static readonly DependencyProperty WorldPositionXProperty = DependencyProperty.RegisterAttached(
        "WorldPositionX", typeof(double), typeof(InfiniteGridView), 
        new FrameworkPropertyMetadata(0.0, WorldPositionXChanged));
        public static void SetWorldPositionX(DependencyObject element, double value) =>
            element.SetValue(WorldPositionXProperty, value);
        public static double GetWorldPositionX(DependencyObject element) =>
            (double)element.GetValue(WorldPositionXProperty);
        private static void WorldPositionXChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var wrapper = (sender as FrameworkElement).Resources["UIElementWpapper"] as UIElementWpapper;
            wrapper.Position = new Point((double)args.NewValue, wrapper.Position.Y);
        }
        // Attached Y position.
        public static readonly DependencyProperty WorldPositionYProperty = DependencyProperty.RegisterAttached(
        "WorldPositionY", typeof(double), typeof(InfiniteGridView), 
        new FrameworkPropertyMetadata(0.0, WorldPositionYChanged));
        public static void SetWorldPositionY(DependencyObject element, double value) =>
            element.SetValue(WorldPositionYProperty, value);
        public static double GetWorldPositionY(DependencyObject element) =>
            (double)element.GetValue(WorldPositionYProperty);
        private static void WorldPositionYChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var wrapper = (sender as FrameworkElement).Resources["UIElementWpapper"] as UIElementWpapper;
            wrapper.Position = new Point(wrapper.Position.X, (double)args.NewValue);
        }
        //Attached Width.
        public static readonly DependencyProperty WorldWidthProperty = DependencyProperty.RegisterAttached(
        "WorldWidth", typeof(double), typeof(InfiniteGridView), 
        new FrameworkPropertyMetadata(1.0, WorldWidthChanged));
        public static void SetWorldWidth(DependencyObject element, double value) =>
            element.SetValue(WorldWidthProperty, value);
        public static double GetWorldWidth(DependencyObject element) =>
            (double)element.GetValue(WorldWidthProperty);
        private static void WorldWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var wrapper = (sender as FrameworkElement).Resources["UIElementWpapper"] as UIElementWpapper;
            wrapper.Size = new Size((double)args.NewValue, wrapper.Size.Height);
        }
        // Attached Height.
        public static readonly DependencyProperty WorldHeightProperty = DependencyProperty.RegisterAttached(
        "WorldHeight", typeof(double), typeof(InfiniteGridView), 
        new FrameworkPropertyMetadata(1.0, WorldHeightChanged));
        public static void SetWorldHeight(DependencyObject element, double value) =>
            element.SetValue(WorldHeightProperty, value);
        public static double GetWorldHeight(DependencyObject element) =>
            (double)element.GetValue(WorldHeightProperty);
        private static void WorldHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var wrapper = (sender as FrameworkElement).Resources["UIElementWpapper"] as UIElementWpapper;
            wrapper.Size = new Size(wrapper.Size.Width, (double)args.NewValue);
        }

        #endregion

        ObservableGrid invisibleChildParent = new ObservableGrid();

        public static InfiniteGridView instance;
        public InfiniteGridView()
        {
            InitializeComponent();
            DataContext = this;

            invisibleChildParent.DataContext = this;
            Children = invisibleChildParent.Children;
            invisibleChildParent.OnChildrenChanged += ChildrenChanged;

            GridCanvas.Loaded += GridCanvasLoaded;
        }

        void GridCanvasLoaded(object sender, RoutedEventArgs e)
        {
            UpdateGrid();

            foreach (var child in childrenWrapped)
                child.Update();
        }

        void ChildrenChanged(DependencyObject added, DependencyObject removed)
        {
            if (added != null)
            {
                var size = new Size(GetWorldWidth(added), GetWorldHeight(added));
                var position = new Point(GetWorldPositionX(added), GetWorldPositionY(added));
                var new_element = new UIElementWpapper(position, size, added as UIElement);
                // To change position and size in attached property changed events.
                (added as FrameworkElement).Resources.Add("UIElementWpapper", new_element);
                Children.Remove(added as UIElement);
                childrenWrapped.Add(new_element);
                var t = VisualTreeHelper.GetParent(added);
                new_element.Update();
            }

            foreach (var wrapped_child in childrenWrapped)
                if (wrapped_child.Element == removed)
                {
                    childrenWrapped.Remove(wrapped_child);
                    return;
                }
        }

        void UpdateGrid()
        {
            GridCanvas.Children.Clear();
            // Vert lines.
            double min_visible_x = -fieldOffset.X - GridCanvas.ActualWidth * 0.5;
            double first_line_offset = - min_visible_x % pixelsInUnit;
            double zero_line_x = GridCanvas.ActualWidth * 0.5 + fieldOffset.X;
            for(double x = first_line_offset; x <= GridCanvas.ActualWidth; x += pixelsInUnit)
            {
                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 0;
                line.Y2 = GridCanvas.ActualHeight;
                line.Stroke = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
                line.StrokeThickness = 1;
                line.IsHitTestVisible = false;
                if (Math.Abs(zero_line_x - x) < pixelsInUnit * 0.1)
                    line.StrokeThickness = 3;
                GridCanvas.Children.Add(line);
            }
            // Horz lines.
            double min_visible_y = fieldOffset.Y - GridCanvas.ActualHeight * 0.5;
            first_line_offset = - min_visible_y % pixelsInUnit;
            double zero_line_y = GridCanvas.ActualHeight * 0.5 - fieldOffset.Y;
            for (double y = first_line_offset; y <= GridCanvas.ActualHeight; y += pixelsInUnit)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = GridCanvas.ActualWidth;
                line.Y1 = y;
                line.Y2 = y;
                line.Stroke = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
                line.StrokeThickness = 1;
                line.IsHitTestVisible = false;
                if (Math.Abs(zero_line_y - y) < pixelsInUnit * 0.1)
                    line.StrokeThickness = 3;
                GridCanvas.Children.Add(line);
            }
        }

        #region Field move

        bool movingField = false;
        Point moveStartPoint;
        Point prevOffset;

        private void MoveField(object sender, MouseEventArgs e)
        {
            if(movingField)
            {
                var mouse_pos = e.GetPosition(GridCanvas);
                var delta = mouse_pos - moveStartPoint;
                delta.Y *= -1;
                fieldOffset = prevOffset + delta;
                UpdateGrid();
                foreach(var child in childrenWrapped)
                    child.Update();
            }   
        }

        private void StartEndMoveField(object sender, MouseButtonEventArgs e)
        {
            movingField = e.MiddleButton == MouseButtonState.Pressed;
            moveStartPoint = e.GetPosition(GridCanvas);
            prevOffset = fieldOffset;
            Mouse.Capture(movingField ? GridCanvas : null);
        }

        #endregion
    }
}
