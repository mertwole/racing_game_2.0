using Editor.FileManager;
using System.Collections.ObjectModel;

namespace Editor.GameEntities
{
    public class Track : ISaveableEntity
    {
        ObservableCollection<HeelKeypoint> keypoints = new ObservableCollection<HeelKeypoint>();
        public ObservableCollection<HeelKeypoint> Keypoints { get => keypoints; }

        public Track()
        {

        }

        public Track(Track prototype)
        {
            keypoints = new ObservableCollection<HeelKeypoint>(prototype.keypoints);
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Track;
        }
    }
}
