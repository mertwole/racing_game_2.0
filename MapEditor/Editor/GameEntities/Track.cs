using Editor.FileManager;
using System.ComponentModel;

namespace Editor.GameEntities
{
    public class Track : ISaveableEntity, INotifyPropertyChanged
    {
        BindingList<HeelKeypoint> keypoints = new BindingList<HeelKeypoint>();
        public BindingList<HeelKeypoint> Keypoints { get => keypoints; }

        BindingList<Curvature> curvatures = new BindingList<Curvature>();
        public BindingList<Curvature> Curvatures { get => curvatures; }

        BindingList<GameObject> gameObjects = new BindingList<GameObject>();
        public BindingList<GameObject> GameObjects { get => gameObjects; }

        TrackParameters parameters = new TrackParameters();
        public TrackParameters Parameters { get => parameters; }
             
        public Track()
        {
            Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
        }

        public Track(Track prototype)
        {
            foreach (var keypoint in prototype.keypoints)
                keypoints.Add(new HeelKeypoint(keypoint));

            foreach (var curvature in prototype.curvatures)
                curvatures.Add(curvature);

            foreach (var game_object in prototype.gameObjects)
                gameObjects.Add(game_object);

            parameters = new TrackParameters(prototype.parameters);
            parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Track;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
