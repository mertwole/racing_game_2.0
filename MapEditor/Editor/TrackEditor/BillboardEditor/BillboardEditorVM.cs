using Editor.Common;
using Editor.GameEntities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Editor.TrackEditor.BillboardEditor
{
    public class DistanceToPixelsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            BillboardEditorVM.instance.DistanceToPixels((double)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }

    public class OffsetToPixelsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            BillboardEditorVM.instance.OffsetToPixels((double)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }

    public class BillboardEditorVM : INotifyPropertyChanged
    {
        BillboardEditorModel model = ModelLocator.GetModel<BillboardEditorModel>();

        public ObservableCollection<GameObject> GameObjects { get => model.GameObjects; }

        // MainCanvas
        public static readonly DependencyProperty MainCanvasProperty =
        DependencyProperty.RegisterAttached(
        "MainCanvas", typeof(Canvas),
        typeof(BillboardEditorVM), new FrameworkPropertyMetadata(OnMainCanvasChanged));

        public static void SetMainCanvas(DependencyObject element, Canvas value) => element.SetValue(MainCanvasProperty, value);
        public static Canvas GetMainCanvas(DependencyObject element) => (Canvas)element.GetValue(MainCanvasProperty);

        static Canvas mainCanvas = null;
        public static void OnMainCanvasChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args) => mainCanvas = obj as Canvas;


        public double DistanceToPixels(double distance) => 
            (distance / model.TrackLength) * mainCanvas.ActualWidth;

        public double PixelsToDistance(double pixels) =>
            (pixels / mainCanvas.ActualWidth) * model.TrackLength;

        public double OffsetToPixels(double offset) =>
            (-offset / model.TrackWidth + 0.5) * mainCanvas.ActualHeight;

        public double PixelsToOffset(double pixels) =>
            (pixels / mainCanvas.ActualHeight - 0.5) * model.TrackWidth;



        GameObject ClosestGameObject(double distance, double offset)
        {
            GameObject closest = null;
            double min_dist = double.PositiveInfinity;

            foreach (var go in model.GameObjects)
            {
                var dist = (go.Offset - offset) * (go.Offset - offset)
                    + (go.RoadDistance - distance) * (go.RoadDistance - distance);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest = go;
                }
            }

            return closest;
        }

        #region Move

        bool movingGameObject = false;

        public ICommand StartMoveGameObject
        {
            get => new RelayCommand((e) =>
            {
                Mouse.Capture(mainCanvas);
                movingGameObject = true;

                var args = e as MouseButtonEventArgs;
                var position = args.GetPosition(mainCanvas);

                var closest_go = ClosestGameObject(
                    PixelsToDistance(position.X), PixelsToOffset(position.Y));

                model.StartMoveGameObject(closest_go);
            });
        }

        public ICommand MoveGameObject
        {
            get => new RelayCommand((e) =>
            {
                if (!movingGameObject)
                    return;

                var args = e as MouseEventArgs;
                var position = args.GetPosition(mainCanvas);

                // Validate position.
                if (position.X > mainCanvas.ActualWidth)
                    position.X = mainCanvas.ActualWidth;
                else if (position.X < 0)
                    position.X = 0;

                if (position.Y > mainCanvas.ActualHeight)
                    position.Y = mainCanvas.ActualHeight;
                else if (position.Y < 0)
                    position.Y = 0;

                model.MoveGameObject(PixelsToDistance(position.X), PixelsToOffset(position.Y));
            });
        }

        public ICommand FinishMoveGameObject
        {
            get => new RelayCommand((e) =>
            {
                Mouse.Capture(null);
                movingGameObject = false;
            });
        }

        #endregion

        #region Drag&drop

        public bool DraggingGameObject { get => draggedGameObject == null; }
        GameObject draggedGameObject = null;

        public ICommand DragGameObjectEnter
        {
            get => new RelayCommand((e) =>
            {
                if (draggedGameObject != null)
                    return;

                var args = e as DragEventArgs;
                var position = args.GetPosition(mainCanvas);
                draggedGameObject = model.AddGameObject(
                    PixelsToDistance(position.X), PixelsToOffset(position.Y));
                OnPropertyChanged("DraggingGameObject");
                model.StartMoveGameObject(draggedGameObject);
            });
        }

        public ICommand DragGameObjectOver
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                var position = args.GetPosition(mainCanvas);

                bool inside_canvas = true;
                if (position.X > mainCanvas.ActualWidth)
                    inside_canvas = false;
                else if (position.X < 0)
                    inside_canvas = false;
                else if (position.Y > mainCanvas.ActualHeight)
                    inside_canvas = false;
                else if (position.Y < 0)
                    inside_canvas = false;

                if(!inside_canvas)
                {
                    // Deny gameObject.
                    model.RemoveGameObject(draggedGameObject);
                    draggedGameObject = null;
                    OnPropertyChanged("DraggingGameObject");
                }
                else // Move gameObject.
                    model.MoveGameObject(
                        PixelsToDistance(position.X), PixelsToOffset(position.Y));
            });
        }

        public ICommand DropGameObject
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;

                draggedGameObject = null;
                OnPropertyChanged("DraggingGameObject");
            });
        }

        #endregion

        #region Remove

        public ICommand RemoveGameObject
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                var position = args.GetPosition(mainCanvas);

                var closest = ClosestGameObject(
                    PixelsToDistance(position.X), PixelsToOffset(position.Y));

                model.RemoveGameObject(closest);
            });
        }

        #endregion

        public static BillboardEditorVM instance;
        public BillboardEditorVM()
        {
            if (instance != null)
                throw new Exception("Trying to create another instance of singleton TrackEditorVM");
            instance = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
