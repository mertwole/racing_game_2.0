using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.HeelEditor
{
    public class HeelKeypoint : INotifyPropertyChanged
    {
        double x;
        double y;

        public double X { get => x; set { x = value; OnPropertyChanged("X"); } }
        public double Y { get => y; set { y = value; OnPropertyChanged("Y"); } }

        public HeelKeypoint(double x, double y)
        {
            X = x; Y = y;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class HeelEditorModel
    {
        ObservableCollection<HeelKeypoint> keypoints = new ObservableCollection<HeelKeypoint>();
        public ObservableCollection<HeelKeypoint> Keypoints { get => keypoints; }

        public void Init(double width)
        {
            keypoints.Add(new HeelKeypoint(0, 10));
            keypoints.Add(new HeelKeypoint(width, 10));
        }

        int DetermineClosestKeypoint(double x, double y)
        {
            double min_dist = double.PositiveInfinity;
            int closest = -1;

            for (int i = 0; i < keypoints.Count; i++)
            {
                var dist = (keypoints[i].X - x) * (keypoints[i].X - x)
                    + (keypoints[i].Y - y) * (keypoints[i].Y - y);

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
            for (int i = 0; i < keypoints.Count - 1; i++)
                if (keypoints[i + 1].X >= position && position >= keypoints[i].X)
                {
                    var t = (position - keypoints[i].X) / (keypoints[i + 1].X - keypoints[i].X);
                    return smoothstep(keypoints[i].Y, keypoints[i + 1].Y, t);
                }

            return 0.0;
        }

        public void AddNewKeypoint(double x, double y)
        {
            // Keep keypoints sorted.
            for(int insert_id = 1; insert_id < keypoints.Count; insert_id++)
                if(keypoints[insert_id - 1].X <= x && keypoints[insert_id].X >= x)
                {
                    keypoints.Insert(insert_id, new HeelKeypoint(x, y));
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
            if (editingKeypointId == 0 || editingKeypointId == keypoints.Count - 1)
            {
                keypoints[0].Y = y;
                keypoints[keypoints.Count - 1].Y = y;

                // Elsewhere ObservableCollection not calls CollectionChanged.
                var kp = keypoints[keypoints.Count - 1];
                keypoints.RemoveAt(keypoints.Count - 1);
                keypoints.Add(kp);

                return;
            }

            keypoints.RemoveAt(editingKeypointId);

            // Keep keypoints sorted.
            for (int insert_id = 1; insert_id < keypoints.Count; insert_id++)
                if (keypoints[insert_id - 1].X <= x && keypoints[insert_id].X >= x)
                {
                    keypoints.Insert(insert_id, new HeelKeypoint(x, y));
                    editingKeypointId = insert_id;
                    return;
                }
        }

        public void EndMoveKeypoint()
        {

        }

        public void RemoveKeypoint(double x, double y)
        {
            var remove_id = DetermineClosestKeypoint(x, y);
            if (remove_id == 0 || remove_id == keypoints.Count - 1)
                return;
            keypoints.RemoveAt(remove_id);
        }
    }
}
