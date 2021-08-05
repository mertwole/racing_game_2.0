using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.TrackEditor.GameObjectLocationEditor
{
    public class GameObjectLocationEditorModel
    {
        public ObservableCollection<GameObject> GameObjects { get => trackEditor.Track.GameObjects; }

        TrackEditorModel trackEditor;
        public GameObjectLocationEditorModel(TrackEditorModel track_editor)
        {
            trackEditor = track_editor;
        }

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

            trackEditor.Dirtied();
        }

        public GameObject AddGameObject(double distance, double offset)
        {
            trackEditor.Dirtied();

            var go = new GameObject(distance, offset);
            GameObjects.Add(go);
            return go;
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            GameObjects.Remove(gameObject);

            trackEditor.Dirtied();
        }
    }
}
