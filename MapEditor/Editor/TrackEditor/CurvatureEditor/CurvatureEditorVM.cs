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
using System.Windows.Media;

namespace Editor.TrackEditor.CurvatureEditor
{
    // Values[0] is size and values[1] is CurvatureEditorVM instance.
    public class SizeToPixelsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            (values[1] as CurvatureEditorVM).SizeToPixels((double)values[0]);

        public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
            => null;
    }

    public class CurvatureEditorVM : IViewModel, INotifyPropertyChanged
    {
        CurvatureEditorModel model;

        public void SetModel(object model)
        {
            this.model = model as CurvatureEditorModel;
            Init();
            OnPropertyChanged("Curvatures");
            OnPropertyChanged("TrackLength");
        }

        public void ProvideModelToRequester(RequestModelEventArgs args) { }

        public bool IsCurvatureNotEditing { get => !model.IsCurvatureEditing; }
        public BindingList<Curvature> Curvatures { get => model?.Curvatures; }

        public double TrackLength { get => model == null ? 100.0 : model.TrackLength; }

        double mainCanvasWidth = 0;

        public ICommand MainCanvasLoaded
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var main_canvas = args.Source as Canvas;
                mainCanvasWidth = main_canvas.ActualWidth;
                UpdateCurvatures();
            });
        }

        public ICommand MainCanvasSizeChanged
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var main_canvas = args.Source as Canvas;
                mainCanvasWidth = main_canvas.ActualWidth;
                UpdateCurvatures();
            });
        }

        public double SizeToPixels(double size) =>
            size / model.TrackLength * mainCanvasWidth;

        double PixelsToSize(double pixels) =>
            pixels / mainCanvasWidth * model.TrackLength;

        enum State
        {
            None,
            EditingCurvature,
            CreatingCurvature
        }
        State state = State.None;

        void Init()
        {
            model.PropertyChanged += (sender, name) => {
                if (name.PropertyName == "IsCurvatureEditing") OnPropertyChanged("IsCurvatureNotEditing");
            };
        }

        void UpdateCurvatures()
        {
            foreach (var curv in Curvatures)
            {
                curv.Start += double.Epsilon;
                curv.Length += double.Epsilon;
            }
        }

        FrameworkElement FindParentByName(FrameworkElement element, string name)
        {
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (parent == null) return null;
            if (parent.Name == name) return parent;
            return FindParentByName(parent, name);
        }

        public ICommand StartCreate
        {
            get => new RelayCommand((e) =>
            {
                state = State.CreatingCurvature;

                var args = e as MouseButtonEventArgs;
                var sender = args.Source as IInputElement;

                var pos = PixelsToSize(args.GetPosition(sender).X);
                model.CreateCurvature(pos);

                Mouse.Capture(sender);
            });
        }

        public ICommand FinishCreateAndEdit
        {
            get => new RelayCommand((e) =>
            {
                switch (state)
                {
                    case State.CreatingCurvature: { model.FinishCurvatureCreate(); break; }
                    case State.EditingCurvature: { model.FinishCurvatureEdit(); break; }
                }

                state = State.None;

                Mouse.Capture(null);
            });
        }

        public ICommand CreateOrEdit
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;
                var sender = args.Source as IInputElement;
                var position = PixelsToSize(args.GetPosition(sender).X);

                switch (state)
                {
                    case State.CreatingCurvature: { model.CreatingCurvature(position); break; }
                    case State.EditingCurvature: { model.EditingCurvature(position); break; }
                }
            });
        }

        public ICommand StartEdit
        {
            get => new RelayCommand((e) =>
            {
                state = State.EditingCurvature;

                var args = e as MouseEventArgs;
                args.Handled = true;

                var root = FindParentByName(args.Source as FrameworkElement, "Root");
                var pos = PixelsToSize(args.GetPosition(root).X);
                model.StartCurvatureEdit(pos);

                Mouse.Capture(root);
            });
        }

        public ICommand DeleteCurvatureAt
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;
                if (args.RightButton == MouseButtonState.Released) return;

                var sender = args.Source;
                var root = FindParentByName(sender as FrameworkElement, "Root");
                var pos = PixelsToSize(Mouse.GetPosition(root).X);
                model.DeleteCurvatureAt(pos);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
