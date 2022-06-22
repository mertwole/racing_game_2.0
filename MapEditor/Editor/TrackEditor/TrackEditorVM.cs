using System.ComponentModel;
using System.Windows.Media;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
using Editor.TrackEditor.ParametersEditor;
using Editor.TrackEditor.TrackPreview;

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
                OnPropertyChanged("TrackPreviewView");
                OnPropertyChanged("ParametersEditorView");

                OnPropertyChanged("Length");
                model.Track.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Length");
            }
        }

        public double Length { get => model == null ? 0.0 : model.Track.Parameters.Length; }

        public CurvatureEditorView CurvatureEditorView { get => model?.CurvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => model?.GameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => model?.HeelEditorView; }
        public TrackPreviewView TrackPreviewView { get => model?.TrackPreviewView; }
        public ParametersEditorView ParametersEditorView { get => model?.ParametersEditorView; }

        public double PointerPositionNormalized { set { if(model != null) model.PointerPositionNormalized = value; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
