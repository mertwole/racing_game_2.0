using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.TrackEditor.HeelEditor
{
    public class HeelEditorModel
    {
        public ObservableCollection<HeelKeypoint> Keypoints { get => trackEditor.Track.Keypoints; }

        TrackEditorModel trackEditor;
        public HeelEditorModel(TrackEditorModel track_editor)
        {
            trackEditor = track_editor;
        }

        public void Init(double width)
        {
            Keypoints.Add(new HeelKeypoint(0, 10));
            Keypoints.Add(new HeelKeypoint(width, 10));
        }

        int DetermineClosestKeypoint(double x, double y)
        {
            double min_dist = double.PositiveInfinity;
            int closest = -1;

            for (int i = 0; i < Keypoints.Count; i++)
            {
                var dist = (Keypoints[i].X - x) * (Keypoints[i].X - x)
                    + (Keypoints[i].Y - y) * (Keypoints[i].Y - y);

                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest = i;
                }
            }

            return closest;
        }

        public double GetHeightByPosition(double position)
        {
            double smoothstep(double a, double b, double t)
            {
                t = t * t * (3.0 - 2.0 * t);
                return a + (b - a) * t;
            }
            // TODO : Optimize - cache i
            for (int i = 0; i < Keypoints.Count - 1; i++)
                if (Keypoints[i + 1].X >= position && position >= Keypoints[i].X)
                {
                    var t = (position - Keypoints[i].X) / (Keypoints[i + 1].X - Keypoints[i].X);
                    return smoothstep(Keypoints[i].Y, Keypoints[i + 1].Y, t);
                }

            return 0.0;
        }

        public void AddNewKeypoint(double x, double y)
        {
            trackEditor.Dirtied();

            // Keep keypoints sorted.
            for(int insert_id = 1; insert_id < Keypoints.Count; insert_id++)
                if(Keypoints[insert_id - 1].X <= x && Keypoints[insert_id].X >= x)
                {
                    Keypoints.Insert(insert_id, new HeelKeypoint(x, y));
                    return;
                } 
        }

        int editingKeypointId = -1;
        public void StartMoveKeypoint(double x, double y)
        {
            editingKeypointId = DetermineClosestKeypoint(x, y);
        }

        public void MoveKeypoint(double x, double y)
        {
            trackEditor.Dirtied();

            if (editingKeypointId == 0 || editingKeypointId == Keypoints.Count - 1)
            {
                Keypoints[0].Y = y;
                Keypoints[Keypoints.Count - 1].Y = y;

                // Elsewhere ObservableCollection not calls CollectionChanged.
                var kp = Keypoints[Keypoints.Count - 1];
                Keypoints.RemoveAt(Keypoints.Count - 1);
                Keypoints.Add(kp);

                return;
            }

            Keypoints.RemoveAt(editingKeypointId);

            // Keep keypoints sorted.
            for (int insert_id = 1; insert_id < Keypoints.Count; insert_id++)
                if (Keypoints[insert_id - 1].X <= x && Keypoints[insert_id].X >= x)
                {
                    Keypoints.Insert(insert_id, new HeelKeypoint(x, y));
                    editingKeypointId = insert_id;
                    return;
                }
        }

        public void EndMoveKeypoint()
        {

        }

        public void RemoveKeypoint(double x, double y)
        {
            trackEditor.Dirtied();

            var remove_id = DetermineClosestKeypoint(x, y);
            if (remove_id == 0 || remove_id == Keypoints.Count - 1)
                return;
            Keypoints.RemoveAt(remove_id);
        }
    }
}
