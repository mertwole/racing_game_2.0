using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Editor.CustomControls
{
    [ContentProperty("Children")]
    public partial class TimelineView : UserControl
    {
        //double timelineLength = 50.0;
        //public double TimelineLength { get => timelineLength; set => timelineLength = value; }

        double editFieldHeight = 500.0;
        public double EditFieldHeight { get => editFieldHeight; set => editFieldHeight = value; }

        double scaleX = 40.0;
        double scaleY = 2.0;

        double fontSize = 10.0;

        double horzScrollSensitivity = 0.3;
        double vertScrollSensitivity = 0.3;

        public static readonly DependencyPropertyKey ChildrenProperty =
        DependencyProperty.RegisterReadOnly(
        "Children",
        typeof(UIElementCollection),
        typeof(TimelineView),
        new PropertyMetadata()
        );
        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        public static DependencyProperty TimelineLengthProperty;
        public double TimelineLength
        {
            get { return (double)GetValue(TimelineLengthProperty); }
            set { SetValue(TimelineLengthProperty, value); }
        }

        public static readonly DependencyProperty PointerPositionNormalizedProperty = DependencyProperty.Register(
        "PointerPositionNormalized", typeof(double),
        typeof(TimelineView), new FrameworkPropertyMetadata());
        public double PointerPositionNormalized
        {
            get { return (double)GetValue(PointerPositionNormalizedProperty); }
            set { SetValue(PointerPositionNormalizedProperty, value); UpdatePointerPos(); }
        }

        void UpdatePointerPos() =>
            Canvas.SetLeft(Pointer, PointerPositionNormalized * TimelineCanvas.Width - Pointer.Width * 0.5);

        void UpdateTimelineLength(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateTimelineLength();
        }

        void UpdateTimelineLength()
        {
            TimelineCanvas.Width = TimelineLength * scaleX;
            ChildrenContainer.Width = TimelineLength * scaleX;

            for (int i = 0; i <= TimelineLength; i++)
            {
                var line = new Line();
                line.X1 = i * scaleX;
                line.X2 = i * scaleX;
                line.Y1 = 0;
                line.Y2 = editFieldHeight * scaleY;
                line.Stroke = new SolidColorBrush(Color.FromArgb(45, 0x7B, 0x7B, 0x7B));
                line.StrokeThickness = 1.0;
                line.IsHitTestVisible = false;

                var text = new TextBlock();
                text.Text = $"{i}.0";
                text.FontSize = fontSize;
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(text, i * scaleX - text.DesiredSize.Width * 0.5);
                Canvas.SetTop(text, 0);

                TimelineCanvas.Children.Add(line);
                TimelineCanvas.Children.Add(text);
            }

            UpdatePointerPos();
        }

        public TimelineView()
        {
            TimelineLengthProperty = DependencyProperty.Register(
                "TimelineLength", typeof(double),
                typeof(TimelineView), new FrameworkPropertyMetadata(100.0, new PropertyChangedCallback(UpdateTimelineLength)));

            InitializeComponent();
            Children = ChildrenContainer.Children;
            TimelineCanvas.Height = editFieldHeight * scaleY;

            UpdateTimelineLength();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                ScrollRegion.ScrollToHorizontalOffset(ScrollRegion.HorizontalOffset + e.Delta * horzScrollSensitivity);
            else
                ScrollRegion.ScrollToVerticalOffset(ScrollRegion.VerticalOffset - e.Delta * vertScrollSensitivity);
        }

        bool movePointer = false;

        private void PointerMove(object sender, MouseEventArgs e)
        {
            if (!movePointer) return;

            var pos = e.GetPosition(TimelineCanvas).X / TimelineCanvas.Width;
            if (pos < 0) pos = 0;
            if (pos > 1) pos = 1;
            PointerPositionNormalized = pos;
        }

        private void PointerEndMove(object sender, MouseButtonEventArgs e)
        {
            movePointer = false;
            Mouse.Capture(null);
        }

        private void PointerStartMove(object sender, MouseButtonEventArgs e)
        {
            movePointer = true;
            Mouse.Capture(PointerInputElement);
        }
    }
}
