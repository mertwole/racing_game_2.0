using Editor.Common;
using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor.TrackEditor.CurvatureEditor
{
    public class CurvatureEditorVM : INotifyPropertyChanged
    {
        CurvatureEditorModel model;// = ModelLocator.GetModel<CurvatureEditorModel>();
        public CurvatureEditorModel Model { set { model = value; Init(); OnPropertyChanged("Curvatures"); } }

        public bool IsCurvatureNotEditing { get => !model.IsCurvatureEditing; }
        public ObservableCollection<Curvature> Curvatures { get => model == null ? null : model.Curvatures; }

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

                model.CreateCurvature(args.GetPosition(sender).X);

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
                var position = args.GetPosition(sender).X;

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
                model.StartCurvatureEdit(args.GetPosition(root).X);

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
                model.DeleteCurvatureAt(Mouse.GetPosition(root).X);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
