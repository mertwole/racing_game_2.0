using System.ComponentModel;
using System.Drawing;

namespace Editor.TrackPreview
{
    public class TrackPreviewVM : INotifyPropertyChanged
    {
        TrackPreviewModel model = MainModel.TrackPreviewModel;

        public Bitmap Preview => model.Preview;

        public TrackPreviewVM()
        {
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Preview")
                    OnPropertyChanged("Preview");
            };

            OnPropertyChanged("Preview");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
