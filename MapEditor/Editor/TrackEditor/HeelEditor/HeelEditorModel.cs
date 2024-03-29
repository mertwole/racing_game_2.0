﻿using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.TrackEditor.HeelEditor
{
    public class HeelEditorModel : INotifyPropertyChanged
    {
        public BindingList<HeelKeypoint> Keypoints { get => trackEditor.Track.Keypoints; }

        double editorHeight = 1.0;
        public double EditorHeight 
        { 
            get => editorHeight; 
            set
            {
                var old_value = editorHeight;
                editorHeight = value;
                EditorHeightChanged(old_value, value);
            } 
        }

        public double TrackLength { get => trackEditor.Track.Parameters.Length; }

        TrackEditorModel trackEditor;
        public HeelEditorModel(TrackEditorModel track_editor)
        {
            trackEditor = track_editor;
            trackEditor.Track.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Parameters")
                    TrackLengthChanged();
            };
        }

        public void Init()
        {
            if (Keypoints.Count != 0)
                return;

            Keypoints.Add(new HeelKeypoint(0, editorHeight * 0.5));
            Keypoints.Add(new HeelKeypoint(TrackLength, editorHeight * 0.5));
        }

        void TrackLengthChanged()
        {
            for(int i = Keypoints.Count - 1; i >= 0; i--)
            {
                if(Keypoints[i].X >= TrackLength)
                    Keypoints.RemoveAt(i);
            }

            var first_height = Keypoints[0].Y;
            Keypoints.Add(new HeelKeypoint(TrackLength, first_height));
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
            var remove_id = DetermineClosestKeypoint(x, y);
            if (remove_id == 0 || remove_id == Keypoints.Count - 1)
                return;
            Keypoints.RemoveAt(remove_id);
        }

        void EditorHeightChanged(double old_value, double new_value)
        {
            double ratio = old_value / new_value;
            for(int i = 0; i < Keypoints.Count; i++)
            {
                Keypoints[i].Y *= ratio;
                if (Keypoints[i].Y > new_value)
                    Keypoints[i].Y = new_value;
            }

            // Elsewhere ObservableCollection not calls CollectionChanged.
            var kp = Keypoints[Keypoints.Count - 1];
            Keypoints.RemoveAt(Keypoints.Count - 1);
            Keypoints.Add(kp);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
