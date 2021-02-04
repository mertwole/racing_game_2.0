using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.TrackEditor
{
    public class TrackEditorModel
    {
        ObservableCollection<GameObject> gameObjects = new ObservableCollection<GameObject>();
        public ObservableCollection<GameObject> GameObjects { get => gameObjects; }

        double trackWidth = 4;
        public double TrackWidth { get => trackWidth; }
        double trackLength = 120;
        public double TrackLength { get => trackLength; }

        GameObject toMove = null;

        public void StartMoveGameObject(GameObject gameObject) => toMove = gameObject;

        public void MoveGameObject(double distance, double offset)
        {
            toMove.RoadDistance = distance;
            toMove.Offset = offset;
        }

        public GameObject AddGameObject(double distance, double offset)
        {
            var go = new GameObject(distance, offset);
            gameObjects.Add(go);
            return go;
        }

        public void RemoveGameObject(GameObject gameObject) 
            => gameObjects.Remove(gameObject);
    }

    public class GameObject : INotifyPropertyChanged
    {
        double roadDistance;
        public double RoadDistance
        {
            get => roadDistance; 
            set { 
                roadDistance = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoadDistance")); 
            }
        }

        double offset;
        public double Offset
        {
            get => offset;
            set
            {
                offset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Offset"));
            }
        }

        public GameObject(double road_distance, double offset)
        {
            RoadDistance = road_distance;
            Offset = offset;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
