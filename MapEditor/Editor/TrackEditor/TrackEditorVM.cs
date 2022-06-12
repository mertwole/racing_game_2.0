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

                model.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "Length")
                        OnPropertyChanged("Length");
                };
            }
        }

        public CurvatureEditorView CurvatureEditorView { get => model?.CurvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => model?.GameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => model?.HeelEditorView; }
        public TrackPreviewView TrackPreviewView { get => model?.TrackPreviewView; }

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
