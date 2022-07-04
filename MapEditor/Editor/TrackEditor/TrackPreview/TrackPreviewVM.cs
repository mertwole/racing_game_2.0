using System.ComponentModel;
using System.Drawing;

namespace Editor.TrackEditor.TrackPreview
{
    public class TrackPreviewVM : IViewModel, INotifyPropertyChanged
    {
        TrackPreviewModel model;

        public void SetModel(object model)
        {
            this.model = model as TrackPreviewModel;
            this.model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Preview")
                    OnPropertyChanged("Preview");
            };

            OnPropertyChanged("Preview");
        }

        public void ProvideModelToRequester(RequestModelEventArgs args) { }

        public Bitmap Preview => model?.Preview;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
