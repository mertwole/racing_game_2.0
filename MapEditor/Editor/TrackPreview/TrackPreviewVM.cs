using System.ComponentModel;
using System.Drawing;

namespace Editor.TrackPreview
{
    public class TrackPreviewVM : INotifyPropertyChanged
    {
        TrackPreviewModel model;
        public TrackPreviewModel Model 
        {
            set
            {
                model = value;
                model.PropertyChanged += (s, e) => 
                { 
                    if(e.PropertyName == "Preview")
                        OnPropertyChanged("Preview"); 
                };
                OnPropertyChanged("Preview");
            }
        }

        public Bitmap Preview => model == null ? null : model.Preview;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
