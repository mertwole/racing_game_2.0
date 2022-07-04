using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.TrackEditor
{
    public class TrackEditorModel : IEditorTabModel, INotifyPropertyChanged
    {
        Track track;
        public Track Track { get => track; }

        double pointerPositionNormalized = 0.0;
        public double PointerPositionNormalized {
            set
            {
                pointerPositionNormalized = value;
                OnPropertyChanged("PointerPositionNormalized");
            }
            get => pointerPositionNormalized; 
        }

        public TrackEditorModel(Track track)
        {
            this.track = track;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
