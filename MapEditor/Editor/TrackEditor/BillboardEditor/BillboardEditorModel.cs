using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.TrackEditor.BillboardEditor
{
    public class BillboardEditorModel
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

    
}
