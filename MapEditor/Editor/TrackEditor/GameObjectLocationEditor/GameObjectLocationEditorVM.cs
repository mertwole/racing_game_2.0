using Editor.Common;
using Editor.FileManager;
using Editor.GameEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor.TrackEditor.GameObjectLocationEditor
{
    // Values[0] is distance and values[1] is GameObjectLocationEditorVM instance.
    public class DistanceToPixelsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            (values[1] as GameObjectLocationEditorVM).DistanceToPixels((double)values[0]);

        public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
            => null;
    }

    // Values[0] is offset and values[1] is GameObjectLocationEditorVM instance.
    public class OffsetToPixelsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            (values[1] as GameObjectLocationEditorVM).OffsetToPixels((double)values[0]);

        public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
            => null;
    }

    public class GameObjectLocationEditorVM : INotifyPropertyChanged
    {
        GameObjectLocationEditorModel model;
        public GameObjectLocationEditorModel Model { 
            set 
            { 
                model = value; 
                OnPropertyChanged("GameObjects");
                OnPropertyChanged("TrackWidth");
            } 
        }

        public ObservableCollection<GameObject> GameObjects { get => model == null ? null : model.GameObjects; }

        public double TrackWidth { get => model == null ? 1.0 : model.TrackWidth; set => model.TrackWidth = value; }

        double mainCanvasWidth = 0;
        double mainCanvasHeight = 0;

        public ICommand MainCanvasLoaded
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var main_canvas = args.Source as Canvas;
                mainCanvasWidth = main_canvas.ActualWidth;
                mainCanvasHeight = main_canvas.ActualHeight;
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
            });
        }

        public double DistanceToPixels(double distance) => 
            (distance / model.TrackLength) * mainCanvasWidth;

        public double PixelsToDistance(double pixels) =>
            (pixels / mainCanvasWidth) * model.TrackLength;

        public double OffsetToPixels(double offset) =>
            (-offset / model.TrackWidth + 0.5) * mainCanvasHeight;

        public double PixelsToOffset(double pixels) =>
            (pixels / mainCanvasHeight - 0.5) * model.TrackWidth;


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

        FrameworkElement FindParentByName(FrameworkElement element, string name)
        {
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (parent == null) return null;
            if (parent.Name == name) return parent;
            return FindParentByName(parent, name);
        }

        #region Move

        bool movingGameObject = false;

        public ICommand StartMoveGameObject
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;
                var root = FindParentByName(args.Source as FrameworkElement, "Root");

                Mouse.Capture(root);
                movingGameObject = true;

                var position = args.GetPosition(root);

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
                var position = args.GetPosition(args.Source as IInputElement);

                // Validate position.
                if (position.X > mainCanvasWidth)
                    position.X = mainCanvasWidth;
                else if (position.X < 0)
                    position.X = 0;

                if (position.Y > mainCanvasHeight)
                    position.Y = mainCanvasHeight;
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

        GameObject GetGameObjectFromDragEventArgs(DragEventArgs args)
        {
            var contents = (HashSet<IContent>)args.Data.GetData(typeof(HashSet<IContent>));
            if (contents == null) return null;

            var files = new List<File>();
            foreach (var content in contents)
                if (content is File file)
                    files.Add(file);

            foreach (var file in files)
                if (file.Content is GameObject go)
                    return go;

            return null;
        }

        public ICommand DragGameObjectEnter
        {
            get => new RelayCommand((e) =>
            {
                if (draggedGameObject != null)
                    return;

                var args = e as DragEventArgs;

                var game_object = GetGameObjectFromDragEventArgs(args);
                if (game_object == null) return;

                var position = args.GetPosition(args.Source as IInputElement);
                draggedGameObject = model.AddGameObject(
                    PixelsToDistance(position.X), PixelsToOffset(position.Y), game_object);

                model.StartMoveGameObject(draggedGameObject);

                OnPropertyChanged("DraggingGameObject");
            });
        }

        public ICommand DragGameObjectOver
        {
            get => new RelayCommand((e) =>
            {
                if (draggedGameObject == null)
                    return;

                var args = e as DragEventArgs;
                var position = args.GetPosition(args.Source as IInputElement);

                bool inside_canvas = true;
                if (position.X > mainCanvasWidth)
                    inside_canvas = false;
                else if (position.X < 0)
                    inside_canvas = false;
                else if (position.Y > mainCanvasHeight)
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
                var root = FindParentByName(args.Source as FrameworkElement, "Root");
                var position = args.GetPosition(root);

                var closest = ClosestGameObject(
                    PixelsToDistance(position.X), PixelsToOffset(position.Y));

                model.RemoveGameObject(closest);
            });
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
