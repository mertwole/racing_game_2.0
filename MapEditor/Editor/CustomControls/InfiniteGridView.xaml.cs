using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Editor.CustomControls
{
    #region Coordinate converters
    // These converters convert control with attached properties
    // InfiniteGridView:Width, InfiniteGridView:Height, InfiniteGridView:WorldPositionX, InfiniteGridView:WorldPositionY
    // to local canvas coords.
    // First parameter is InfiniteGridView instance and 2nd is current billboard.
    public class CanvasLeftConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var grid_view = value[0] as InfiniteGridView;
            var element = grid_view.MainItemsControl.ItemContainerGenerator.ContainerFromItem(value[1]);
            // Because previous command returns ContentPresenter.
            if (VisualTreeHelper.GetChildrenCount(element) != 0)
                element = VisualTreeHelper.GetChild(element, 0);
            return grid_view.GetLocalLeft(element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class CanvasBottomConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var grid_view = value[0] as InfiniteGridView;
            var element = grid_view.MainItemsControl.ItemContainerGenerator.ContainerFromItem(value[1]);
            // Because previous command returns ContentPresenter.
            if (VisualTreeHelper.GetChildrenCount(element) != 0) 
                element = VisualTreeHelper.GetChild(element, 0);

            return grid_view.GetLocalBottom(element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class CanvasWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var grid_view = value[0] as InfiniteGridView;
            var element = grid_view.MainItemsControl.ItemContainerGenerator.ContainerFromItem(value[1]);
            // Because previous command returns ContentPresenter.
            if (VisualTreeHelper.GetChildrenCount(element) != 0)
                element = VisualTreeHelper.GetChild(element, 0);
            return grid_view.GetLocalWidth(element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class CanvasHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var grid_view = value[0] as InfiniteGridView;
            var element = grid_view.MainItemsControl.ItemContainerGenerator.ContainerFromItem(value[1]);
            // Because previous command returns ContentPresenter.
            if (VisualTreeHelper.GetChildrenCount(element) != 0)
                element = VisualTreeHelper.GetChild(element, 0);
            return grid_view.GetLocalHeight(element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
    #endregion

    public partial class InfiniteGridView : UserControl, INotifyPropertyChanged
    {
        // Offset in pixels relative to center.
        Point fieldOffset = new Point(0, 0);
        double pixelsInUnit = 30;

        #region Methods for conversion to local coords
        public double GetLocalLeft(DependencyObject element)
        {
            var position = new Point(GetWorldPositionX(element), GetWorldPositionY(element));
            var size = new Size(GetWorldWidth(element), GetWorldHeight(element));
            var left_bottom = (Point)(position - new Point(size.Width * 0.5, size.Height * 0.5));
            return WorldToScreenSpace(left_bottom).X;
        }

        public double GetLocalBottom(DependencyObject element)
        {
            var position = new Point(GetWorldPositionX(element), GetWorldPositionY(element));
            var size = new Size(GetWorldWidth(element), GetWorldHeight(element));
            var left_bottom = (Point)(position - new Point(size.Width * 0.5, size.Height * 0.5));
            return WorldToScreenSpace(left_bottom).Y;
        }

        public double GetLocalWidth(DependencyObject element)
        {
            var width = GetWorldWidth(element);
            return WorldSizeToScreenSpace(width);
        }

        public double GetLocalHeight(DependencyObject element)
        {
            var height = GetWorldHeight(element);
            return WorldSizeToScreenSpace(height);
        }
        #endregion

        public Point WorldToScreenSpace(Point world_pos) =>
            new Point(world_pos.X * pixelsInUnit + fieldOffset.X + GridCanvas.ActualWidth * 0.5,
                world_pos.Y * pixelsInUnit + fieldOffset.Y + GridCanvas.ActualHeight * 0.5);

        public double WorldSizeToScreenSpace(double world_size) =>
            world_size * pixelsInUnit;

        #region Attached properties
        // Attached X position.
        public static readonly DependencyProperty WorldPositionXProperty = DependencyProperty.RegisterAttached(
            "WorldPositionX", typeof(double), typeof(InfiniteGridView));
        public static void SetWorldPositionX(DependencyObject element, double value) =>
            element.SetValue(WorldPositionXProperty, value);
        public static double GetWorldPositionX(DependencyObject element) =>
            (double)element.GetValue(WorldPositionXProperty);
        // Attached Y position.
        public static readonly DependencyProperty WorldPositionYProperty = DependencyProperty.RegisterAttached(
            "WorldPositionY", typeof(double), typeof(InfiniteGridView));
        public static void SetWorldPositionY(DependencyObject element, double value) =>
            element.SetValue(WorldPositionYProperty, value);
        public static double GetWorldPositionY(DependencyObject element) =>
            (double)element.GetValue(WorldPositionYProperty);
        //Attached Width.
        public static readonly DependencyProperty WorldWidthProperty = DependencyProperty.RegisterAttached(
            "WorldWidth", typeof(double), typeof(InfiniteGridView));
        public static void SetWorldWidth(DependencyObject element, double value) =>
            element.SetValue(WorldWidthProperty, value);
        public static double GetWorldWidth(DependencyObject element) =>
            (double)element.GetValue(WorldWidthProperty);
        // Attached Height.
        public static readonly DependencyProperty WorldHeightProperty = DependencyProperty.RegisterAttached(
            "WorldHeight", typeof(double), typeof(InfiniteGridView));
        public static void SetWorldHeight(DependencyObject element, double value) =>
            element.SetValue(WorldHeightProperty, value);
        public static double GetWorldHeight(DependencyObject element) =>
            (double)element.GetValue(WorldHeightProperty);
        #endregion

        public static readonly DependencyProperty ItemsSourceProperety =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(InfiniteGridView));
        public IEnumerable ItemsSource { get => itemsSource; set => itemsSource = value; }
        IEnumerable itemsSource;
        
        public static readonly DependencyProperty ItemsTemplateProperety =
            DependencyProperty.Register("ItemsTemplate", typeof(DataTemplate), typeof(InfiniteGridView));
        public DataTemplate ItemsTemplate { get => itemsTemplate; set => itemsTemplate = value; }
        DataTemplate itemsTemplate;

        public InfiniteGridView()
        {
            DataContext = this;
            InitializeComponent();

            GridCanvas.Loaded += (s, e) => UpdateGrid();
            MainItemsControl.Loaded += (s, e) => MainItemsControlLoaded();
        }

        void MainItemsControlLoaded()
        {
            if(MainItemsControl.ItemsSource is INotifyCollectionChanged items_source)
            {
                items_source.CollectionChanged += ItemsChanged;
                ItemsLoaded(MainItemsControl.ItemsSource);
            }
        }

        Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> itemChangedEventHandlers 
            = new Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler>();

        void ItemsLoaded(IEnumerable collection)
        {
            foreach(var item in collection)
                if(item is INotifyPropertyChanged item_observable)
                {
                    PropertyChangedEventHandler handler = (s, ev) => TriggerUpdateItemsInGrid();
                    item_observable.PropertyChanged += handler;
                    itemChangedEventHandlers.Add(item_observable, handler);
                }
        }

        void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var added in e.NewItems)
                if (added is INotifyPropertyChanged added_observable)
                {
                    PropertyChangedEventHandler handler = (s, ev) => TriggerUpdateItemsInGrid();
                    added_observable.PropertyChanged += handler;
                    itemChangedEventHandlers.Add(added_observable, handler);
                }
                    
            foreach(var removed in e.OldItems)
                if (removed is INotifyPropertyChanged removed_observable)
                {
                    var handler = itemChangedEventHandlers[removed_observable];
                    removed_observable.PropertyChanged -= handler;
                    itemChangedEventHandlers.Remove(removed_observable);
                }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // Property that changes every time the grid moves/scales so property multibound to it
        // will update every time the grid needs to be redrawn.
        public bool UpdateItemsInGrid { get => true; }

        void TriggerUpdateItemsInGrid() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UpdateItemsInGrid"));

        void UpdateGrid()
        {
            TriggerUpdateItemsInGrid();

            GridCanvas.Children.Clear();
            // Vert lines.
            double min_visible_x = -fieldOffset.X - GridCanvas.ActualWidth * 0.5;
            double first_line_offset = -min_visible_x % pixelsInUnit;
            double zero_line_x = GridCanvas.ActualWidth * 0.5 + fieldOffset.X;
            for (double x = first_line_offset; x <= GridCanvas.ActualWidth; x += pixelsInUnit)
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
            first_line_offset = -min_visible_y % pixelsInUnit;
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

        void Zoom(object sender, MouseWheelEventArgs e)
         {
            var old_pixelsInUnit = pixelsInUnit;
            pixelsInUnit *= 1.0 + e.Delta * 0.001;

            if (pixelsInUnit < 10.0)
                pixelsInUnit = 10.0;
            else if (pixelsInUnit > 200.0)
                pixelsInUnit = 200.0;

            var scale_factor = pixelsInUnit / old_pixelsInUnit;
            fieldOffset = new Point(fieldOffset.X * scale_factor, fieldOffset.Y * scale_factor);

            UpdateGrid();
        }

        bool movingField = false;
        Point moveStartPoint;
        Point prevOffset;

        void MoveField(object sender, MouseEventArgs e)
        {
            if (!movingField) return;

            var mouse_pos = e.GetPosition(GridCanvas);
            var delta = mouse_pos - moveStartPoint;
            delta.Y *= -1;
            fieldOffset = prevOffset + delta;
            UpdateGrid();
        }

        void StartEndMoveField(object sender, MouseButtonEventArgs e)
        {
            movingField = e.MiddleButton == MouseButtonState.Pressed;
            moveStartPoint = e.GetPosition(GridCanvas);
            prevOffset = fieldOffset;
            Mouse.Capture(movingField ? GridCanvas : null);
        }
    }
}
