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
        public double TrackWidth 
        { 
            get => trackWidth; 
            set
            {
                trackWidth = value;
                TrackWidthChanged(value);
            }
        }

        public double TrackLength { get => trackEditor.Track.Length; }

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

        void TrackWidthChanged(double new_value)
        {
            double ratio = trackWidth / new_value;
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Offset *= ratio;
                if (GameObjects[i].Offset > new_value * 0.5)
                    GameObjects[i].Offset = new_value * 0.5;
                else if (GameObjects[i].Offset < -new_value * 0.5)
                    GameObjects[i].Offset = -new_value * 0.5;
            }
        }
    }
}
