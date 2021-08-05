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

        public GameObject AddGameObject(double distance, double offset, GameObject game_object)
        {
            game_object.Offset = offset;
            game_object.RoadDistance = distance;

            var new_go = new GameObject(game_object);
            GameObjects.Add(new_go);

            trackEditor.Dirtied();

            return new_go;
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            GameObjects.Remove(gameObject);

            trackEditor.Dirtied();
        }
    }
}
