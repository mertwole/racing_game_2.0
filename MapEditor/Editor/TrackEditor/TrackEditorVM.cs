using System.ComponentModel;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
using Editor.TrackEditor.ParametersEditor;
using Editor.TrackEditor.TrackPreview;

namespace Editor.TrackEditor
{
    class TrackEditorVM : IViewModel, INotifyPropertyChanged
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

        public void SetModel(object model)
        {
            this.model = model as TrackEditorModel;
            OnPropertyChanged("CurvatureEditorView");
            OnPropertyChanged("GameObjectLocationEditorView");
            OnPropertyChanged("HeelEditorView");
            OnPropertyChanged("TrackPreviewView");
            OnPropertyChanged("ParametersEditorView");

            OnPropertyChanged("Length");
            this.model.Track.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Length");
        }

        public void ProvideModelToRequester(RequestModelEventArgs args)
        {
            if (args.Requester is ParametersEditorVM)
                args.Requester.SetModel(new ParametersEditorModel(model.Track));
        }

        public double Length { get => model == null ? 0.0 : model.Track.Parameters.Length; }

        public CurvatureEditorView CurvatureEditorView { get => model?.CurvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => model?.GameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => model?.HeelEditorView; }
        public TrackPreviewView TrackPreviewView { get => model?.TrackPreviewView; }

        public double PointerPositionNormalized { set { if(model != null) model.PointerPositionNormalized = value; } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
