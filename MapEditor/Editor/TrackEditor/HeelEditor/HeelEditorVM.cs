using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using Editor.Common;
using System.Windows.Data;
using System.Globalization;
using System;
using Editor.GameEntities;

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

    class HeelEditorVM : INotifyPropertyChanged
    {
        static HeelEditorModel model = TrackEditorModel.HeelEditorModel;

        public ObservableCollection<HeelKeypoint> Keypoints { get => model.Keypoints; }

        bool editingKeypoint = false;

        public ICommand CreateNewKeypoint
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                if (args.ClickCount == 2)
                {
                    var position = args.GetPosition(mainCanvas);

                    model.AddNewKeypoint(position.X, mainCanvas.ActualHeight - position.Y);
                }
            });
        }

        public ICommand StartMoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                Mouse.Capture(mainCanvas);
                editingKeypoint = true;

                var args = e as MouseButtonEventArgs;
                var position = args.GetPosition(mainCanvas);

                model.StartMoveKeypoint(position.X, mainCanvas.ActualHeight - position.Y);
            });
        }

        public ICommand MoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                if (!editingKeypoint)
                    return;

                var args = e as MouseEventArgs;
                var pos = args.GetPosition(mainCanvas);
                // Validate position.
                if (pos.Y < 0) pos.Y = 0;
                else if (pos.Y > mainCanvas.ActualHeight) pos.Y = mainCanvas.ActualHeight;

                if (pos.X < 0) pos.X = 0;
                else if (pos.X > mainCanvas.ActualWidth) pos.X = mainCanvas.ActualWidth;

                model.MoveKeypoint(pos.X, mainCanvas.ActualHeight - pos.Y);
            });
        }

        public ICommand FinishMoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                Mouse.Capture(null);
                editingKeypoint = false;

                model.EndMoveKeypoint();
            });
        }

        public ICommand RemoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                var position = args.GetPosition(mainCanvas);

                model.RemoveKeypoint(position.X, mainCanvas.ActualHeight - position.Y);
            });
        }

        // MainCanvas
        public static readonly DependencyProperty MainCanvasProperty =
        DependencyProperty.RegisterAttached(
        "MainCanvas", typeof(Canvas),
        typeof(HeelEditorVM), new FrameworkPropertyMetadata(OnMainCanvasChanged));

        public static void SetMainCanvas(DependencyObject element, Canvas value) => element.SetValue(MainCanvasProperty, value);
        public static Canvas GetMainCanvas(DependencyObject element) => (Canvas)element.GetValue(MainCanvasProperty);

        static Canvas mainCanvas = null;
        public static void OnMainCanvasChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            mainCanvas = obj as Canvas;
            instance.CanvasCreated();
        }

        static HeelEditorVM instance = null;
        public HeelEditorVM()
        {
            if (instance != null)
                throw new System.Exception("Trying to create another instance of singleton HeelEditorVM");
            instance = this;

            model.Keypoints.CollectionChanged += (s, e) => UpdateGraph();
        }

        // Graph rendering
        List<LineSegment> graphPoints = new List<LineSegment>();
        public List<LineSegment> GraphPoints { get => graphPoints; }

        void CanvasCreated()
        {
            mainCanvas.Loaded += (s, e) => {
                model.Init(mainCanvas.ActualWidth);

                for (int i = 0; i < mainCanvas.ActualWidth; i++)
                    graphPoints.Add(new LineSegment(new Point(i, 0.0), true));

                graphPoints[0].IsStroked = false;

                graphPoints.Add(new LineSegment(new Point(mainCanvas.ActualWidth, mainCanvas.ActualHeight), false));
                graphPoints.Add(new LineSegment(new Point(0, mainCanvas.ActualHeight), false));

                UpdateGraph();
            };
        }

        void UpdateGraph()
        {
            for (int i = 0; i < graphPoints.Count - 2; i++)
            {
                var seg = graphPoints[i];
                seg.Point = new Point(seg.Point.X, mainCanvas.ActualHeight - model.GetHeightByPosition(seg.Point.X));
            }

            OnPropertyChanged("GraphPoints");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
