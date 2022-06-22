using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.TrackEditor.GameObjectLocationEditor
{
    public class GameObjectLocationEditorModel
    {
        public BindingList<GameObject> GameObjects { get => trackEditor.Track.GameObjects; }

        TrackEditorModel trackEditor;
        public GameObjectLocationEditorModel(TrackEditorModel track_editor)
        {
            trackEditor = track_editor;

            trackEditor.Track.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == "Parameters")
                    TrackLengthChanged();
            };
        }

        double trackWidth = 4;
        public double TrackWidth 
        { 
            get => trackWidth; 
            set
            {
                trackWidth = value;
                TrackWidthChanged();
            }
        }

        public double TrackLength { get => trackEditor.Track.Parameters.Length; }

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

        void TrackWidthChanged()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                // Touch offset param to update positions in view.
                GameObjects[i].Offset += double.Epsilon;

                if (GameObjects[i].Offset > trackWidth * 0.5)
                    GameObjects[i].Offset = trackWidth * 0.5;
                else if (GameObjects[i].Offset < -trackWidth * 0.5)
                    GameObjects[i].Offset = -trackWidth * 0.5;
            }

            trackEditor.Dirtied();
        }

        public void TrackLengthChanged()
        {
            for (int i = GameObjects.Count - 1; i >= 0; i--)
            {
                if (GameObjects[i].RoadDistance > TrackLength)
                    GameObjects.RemoveAt(i);
                else {
                    // Touch distance param to update positions in view.
                    GameObjects[i].RoadDistance += double.Epsilon;
                }
            }

            trackEditor.Dirtied();
        }
    }
}
