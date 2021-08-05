using Editor.FileManager;
using System.Collections.ObjectModel;

namespace Editor.GameEntities
{
    public class Track : ISaveableEntity
    {
        ObservableCollection<HeelKeypoint> keypoints = new ObservableCollection<HeelKeypoint>();
        public ObservableCollection<HeelKeypoint> Keypoints { get => keypoints; }

        ObservableCollection<Curvature> curvatures = new ObservableCollection<Curvature>();
        public ObservableCollection<Curvature> Curvatures { get => curvatures; }

        ObservableCollection<GameObject> gameObjects = new ObservableCollection<GameObject>();
        public ObservableCollection<GameObject> GameObjects { get => gameObjects; }

        public Track()
        {

        }

        public Track(Track prototype)
        {
            foreach (var keypoint in prototype.keypoints)
                keypoints.Add(new HeelKeypoint(keypoint));

            foreach (var curvature in prototype.curvatures)
                curvatures.Add(curvature);

            foreach (var game_object in prototype.gameObjects)
                gameObjects.Add(game_object);
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Track;
        }
    }
}
