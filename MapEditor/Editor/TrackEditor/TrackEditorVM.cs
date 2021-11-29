using System.ComponentModel;
using System.Windows.Media;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
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

                OnPropertyChanged("Length");
                OnPropertyChanged("MainColor");
                OnPropertyChanged("SecondaryColor");
            }
        }

        public CurvatureEditorView CurvatureEditorView { get => model == null ? null : model.CurvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => model == null ? null : model.GameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => model == null ? null : model.HeelEditorView; }
        public TrackPreviewView TrackPreviewView { get => model == null ? null : model.TrackPreviewView; }

        public double PointerPositionNormalized { set { if(model != null) model.PointerPositionNormalized = value; } }

        public Color MainColor 
        { 
            get => model == null ? Color.FromRgb(0, 0, 0) : model.MainColor;
            set { if (model != null) model.MainColor = value; }
        }

        public Color SecondaryColor
        {
            get => model == null ? Color.FromRgb(0, 0, 0) : model.SecondaryColor;
            set { if (model != null) model.SecondaryColor = value; }
        }

        public double Length
        {
            get => model == null ? 0.0 : model.Length;
            set { if (model != null) model.Length = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
