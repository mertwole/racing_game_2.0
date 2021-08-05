using System.ComponentModel;
using System.Windows.Input;
using Editor.Common;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;

namespace Editor.TrackEditor
{
    class TrackEditorVM : INotifyPropertyChanged
    {
        TrackEditorModel model;
        public TrackEditorModel Model { 
            set
            {
                model = value;
                OnPropertyChanged("CurvatureEditorView");
                OnPropertyChanged("GameObjectLocationEditorView");
                OnPropertyChanged("HeelEditorView");
            }
        }

        public CurvatureEditorView CurvatureEditorView { get => model == null ? null : model.CurvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => model == null ? null : model.GameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => model == null ? null : model.HeelEditorView; }

        public ICommand ApplyChanges
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyboardEventArgs;

                if ((e as KeyEventArgs).Key == Key.S && args.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                    model.ApplyChanges();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
