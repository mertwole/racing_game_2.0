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

        BindingList<PositionedGameObject> gameObjects = new BindingList<PositionedGameObject>();
        public BindingList<PositionedGameObject> GameObjects { get => gameObjects; }

        TrackParameters parameters = new TrackParameters();
        public TrackParameters Parameters { get => parameters; }

        public Track()
        {
            UpdateOnPropertyChanged();
        }

        public Track(Track prototype)
        {
            foreach (var keypoint in prototype.keypoints)
                keypoints.Add(new HeelKeypoint(keypoint));

            foreach (var curvature in prototype.curvatures)
                curvatures.Add(new Curvature(curvature));

            foreach (var game_object in prototype.gameObjects)
                gameObjects.Add(new PositionedGameObject(game_object));

            parameters = new TrackParameters(prototype.parameters);

            UpdateOnPropertyChanged();
        }

        void UpdateOnPropertyChanged()
        {
            Parameters.PropertyChanged += (s, e) => OnPropertyChanged("Parameters");
            GameObjects.ListChanged += (s, e) => OnPropertyChanged("GameObjects");
            Curvatures.ListChanged += (s, e) => OnPropertyChanged("Curvatures");
            Keypoints.ListChanged += (s, e) => OnPropertyChanged("Keypoints");
        }

        public ISaveableEntity Clone()
        {
            return new Track(this);
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
