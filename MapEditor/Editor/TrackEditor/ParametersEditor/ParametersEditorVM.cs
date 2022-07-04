using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.TrackEditor.ParametersEditor
{
    class ParametersEditorVM : IViewModel, INotifyPropertyChanged
    {
        ParametersEditorModel model;

        public void SetModel(object model)
        {
            this.model = model as ParametersEditorModel;
            OnPropertyChanged("Parameters");
            this.model.Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
        }

        public void ProvideModelToRequester(RequestModelEventArgs args) { }

        public TrackParameters Parameters { get => model?.Parameters; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
