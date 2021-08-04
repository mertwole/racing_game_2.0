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
        HeelEditorModel model = ModelLocator.GetModel<HeelEditorModel>();
        //public HeelEditorModel Model { set => model = value; Init(); }

        public ObservableCollection<HeelKeypoint> Keypoints { get => model.Keypoints; }

        bool editingKeypoint = false;

        double mainCanvasWidth = 0;
        double mainCanvasHeight = 0;

        public HeelEditorVM()
        {
            model.Keypoints.CollectionChanged += (s, e) => UpdateGraph();
        }

        void Init()
        {
            model.Keypoints.CollectionChanged += (s, e) => UpdateGraph();
        }

        // Graph rendering
        List<LineSegment> graphPoints = new List<LineSegment>();
        public List<LineSegment> GraphPoints { get => graphPoints; }

        void UpdateGraph()
        {
            for (int i = 0; i < graphPoints.Count - 2; i++)
            {
                var seg = graphPoints[i];
                seg.Point = new Point(seg.Point.X, mainCanvasHeight - model.GetHeightByPosition(seg.Point.X));
            }

            OnPropertyChanged("GraphPoints");
        }

        public ICommand MainCanvasLoaded
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var main_canvas = args.Source as Canvas;
                mainCanvasWidth = main_canvas.ActualWidth;
                mainCanvasHeight = main_canvas.ActualHeight;

                model.Init(mainCanvasWidth);

                for (int i = 0; i < mainCanvasWidth; i++)
                    graphPoints.Add(new LineSegment(new Point(i, 0.0), true));

                graphPoints[0].IsStroked = false;

                graphPoints.Add(new LineSegment(new Point(mainCanvasWidth, mainCanvasHeight), false));
                graphPoints.Add(new LineSegment(new Point(0, mainCanvasHeight), false));

                UpdateGraph();
            });
        }

        public ICommand MainCanvasSizeChanged
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var main_canvas = args.Source as Canvas;
                mainCanvasWidth = main_canvas.ActualWidth;
                mainCanvasHeight = main_canvas.ActualHeight;

                model.Init(mainCanvasWidth);

                graphPoints.Clear();

                for (int i = 0; i < mainCanvasWidth; i++)
                    graphPoints.Add(new LineSegment(new Point(i, 0.0), true));

                graphPoints[0].IsStroked = false;

                graphPoints.Add(new LineSegment(new Point(mainCanvasWidth, mainCanvasHeight), false));
                graphPoints.Add(new LineSegment(new Point(0, mainCanvasHeight), false));

                UpdateGraph();
            });
        }

        FrameworkElement FindParentByName(FrameworkElement element, string name)
        {
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (parent == null) return null;
            if (parent.Name == name) return parent;
            return FindParentByName(parent, name);
        }

        public ICommand CreateNewKeypoint
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                if (args.ClickCount == 2)
                {
                    var root = FindParentByName(args.Source as FrameworkElement, "Root");
                    var position = args.GetPosition(root);

                    model.AddNewKeypoint(position.X, mainCanvasHeight - position.Y);
                }
            });
        }

        public ICommand StartMoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                var root = FindParentByName(args.Source as FrameworkElement, "Root");
                var position = args.GetPosition(root);

                Mouse.Capture(root);
                editingKeypoint = true;

                model.StartMoveKeypoint(position.X, mainCanvasHeight - position.Y);
            });
        }

        public ICommand MoveKeypoint
        {
            get => new RelayCommand((e) =>
            {
                if (!editingKeypoint)
                    return;

                var args = e as MouseEventArgs;
                var root = FindParentByName(args.Source as FrameworkElement, "Root");
                if((args.Source as FrameworkElement).Name == "Root")
                    root = args.Source as FrameworkElement;
                var pos = args.GetPosition(root);
                // Validate position.
                if (pos.Y < 0) pos.Y = 0;
                else if (pos.Y > mainCanvasHeight) pos.Y = mainCanvasHeight;

                if (pos.X < 0) pos.X = 0;
                else if (pos.X > mainCanvasWidth) pos.X = mainCanvasWidth;

                model.MoveKeypoint(pos.X, mainCanvasHeight - pos.Y);
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
                var root = FindParentByName(args.Source as FrameworkElement, "Root");
                var position = args.GetPosition(root);

                model.RemoveKeypoint(position.X, mainCanvasHeight - position.Y);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
