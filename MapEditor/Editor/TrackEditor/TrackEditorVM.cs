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

                OnPropertyChanged("Length");
                model.Track.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Length");
            }
        }

        public void SetModel(object model)
        {
            this.model = model as TrackEditorModel;

            OnPropertyChanged("Length");
            this.model.Track.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Length");
        }

        public void ProvideModelToRequester(RequestModelEventArgs args)
        {
            if (args.Requester is ParametersEditorVM)
                args.Requester.SetModel(new ParametersEditorModel(model.Track));
            else if (args.Requester is CurvatureEditorVM)
                args.Requester.SetModel(new CurvatureEditorModel(model));
            else if (args.Requester is GameObjectLocationEditorVM)
                args.Requester.SetModel(new GameObjectLocationEditorModel(model));
            else if (args.Requester is HeelEditorVM)
                args.Requester.SetModel(new HeelEditorModel(model));
            else if (args.Requester is TrackPreviewVM)
                args.Requester.SetModel(new TrackPreviewModel(model));
        }

        public double Length { get => model == null ? 0.0 : model.Track.Parameters.Length; }

        public double PointerPositionNormalized { set { if(model != null) model.PointerPositionNormalized = value; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
