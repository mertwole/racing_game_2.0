using Editor.Common;
using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.TrackEditor.CurvatureEditor
{
    public class CurvatureEditorVM : INotifyPropertyChanged
    {
        CurvatureEditorModel model = TrackEditorModel.CurvatureEditorModel;

        public bool IsCurvatureNotEditing { get => !model.IsCurvatureEditing; }
        public ObservableCollection<Curvature> Curvatures { get => model.Curvatures; }

        enum State
        {
            None,
            EditingCurvature,
            CreatingCurvature
        }
        State state = State.None;

        public ICommand StartCreate
        {
            get => new RelayCommand((e) =>
            {
                state = State.CreatingCurvature;

                var args = e as MouseButtonEventArgs;

                model.CreateCurvature(args.GetPosition(mainCanvas).X);

                Mouse.Capture(mainCanvas);
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
                var position = args.GetPosition(mainCanvas).X;

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

                model.StartCurvatureEdit(args.GetPosition(mainCanvas).X);

                Mouse.Capture(mainCanvas);
            });
        }

        public ICommand DeleteCurvatureAt
        {
            get => new RelayCommand((e) =>
            {
                model.DeleteCurvatureAt(Mouse.GetPosition(mainCanvas).X);
            });
        }

        // MainCanvas
        public static readonly DependencyProperty MainCanvasProperty =
        DependencyProperty.RegisterAttached(
        "MainCanvas", typeof(Canvas),
        typeof(CurvatureEditorVM), new FrameworkPropertyMetadata(OnMainCanvasChanged));

        public static void SetMainCanvas(DependencyObject element, DataGrid value) => element.SetValue(MainCanvasProperty, value);
        public static Canvas GetMainCanvas(DependencyObject element) => (Canvas)element.GetValue(MainCanvasProperty);

        static Canvas mainCanvas = null;
        public static void OnMainCanvasChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args) => mainCanvas = obj as Canvas;

        public CurvatureEditorVM()
        {
            model.PropertyChanged += (sender, name) => { 
                if (name.PropertyName == "IsCurvatureEditing") OnPropertyChanged("IsCurvatureNotEditing"); 
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
