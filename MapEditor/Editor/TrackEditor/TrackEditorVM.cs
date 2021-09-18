using System.ComponentModel;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
