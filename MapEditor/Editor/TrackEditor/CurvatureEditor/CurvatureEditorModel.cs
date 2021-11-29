using Editor.GameEntities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Editor.TrackEditor.CurvatureEditor
{
    public class CurvatureEditorModel : INotifyPropertyChanged
    {
        public ObservableCollection<Curvature> Curvatures { get => trackEditor.Track.Curvatures; }
        bool isCurvatureEditing = false;
        public bool IsCurvatureEditing { 
            get => isCurvatureEditing; 
            private set { isCurvatureEditing = value; OnPropertyChanged("IsCurvatureEditing"); } 
        }

        public double TrackLength { get => trackEditor.Track.Length; }

        TrackEditorModel trackEditor;
        public CurvatureEditorModel(TrackEditorModel track_editor)
        {
            trackEditor = track_editor;
            trackEditor.Track.Curvatures.CollectionChanged += CurvaturesChanged;
            foreach (Curvature curv in trackEditor.Track.Curvatures)
                curv.PropertyChanged += (s, _) => trackEditor.Dirtied();
        }

        private void CurvaturesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                foreach(Curvature added in e.NewItems)
                    added.PropertyChanged += (s, _) => trackEditor.Dirtied();
        }

        public void CreateCurvature(double position)
        {
            foreach (var curv in Curvatures)
                if (curv.End > position && curv.Start < position)
                    return;

            IsCurvatureEditing = true;
            curvatureStart = position;
            Curvatures.Add(new Curvature(position, 0.0, 0.1));
            curvatureEditingId = Curvatures.Count - 1;

            trackEditor.Dirtied();
        }

        public void CreatingCurvature(double position)
        {
            // Just edit newly created curvature.
            EditCurvature(position);
        }

        public void StartCurvatureEdit(double position)
        {
            IsCurvatureEditing = true;
            var closest_curvature_edge = double.PositiveInfinity;
            int closest_curvature_id = -1;

            for(int i = 0; i < Curvatures.Count; i++)
            {
                var curvature = Curvatures[i];

                var closest_dist = Math.Abs(position - closest_curvature_edge);
                var start_dist = Math.Abs(position - curvature.Start);

                // When curv_i.End = curv_j.Start select curvature that contains position.
                if (
                    closest_dist > start_dist
                    ||
                    (closest_dist == start_dist
                    && curvature.Start <= position && curvature.End >= position)
                )
                {
                    closest_curvature_edge = curvature.Start;
                    curvatureStart = curvature.End;
                    closest_curvature_id = i;
                }

                closest_dist = Math.Abs(position - closest_curvature_edge);
                var end_dist = Math.Abs(position - curvature.End);

                if (
                    closest_dist > end_dist
                    ||
                    (closest_dist == end_dist
                    && curvature.Start <= position && curvature.End >= position)
                )
                {
                    closest_curvature_edge = curvature.End;
                    curvatureStart = curvature.Start;
                    closest_curvature_id = i;
                }
            }

            curvatureEditingId = closest_curvature_id;
        }

        public void EditingCurvature(double position)
        {
            EditCurvature(position);
        }

        double curvatureStart;
        int curvatureEditingId = -1;

        void EditCurvature(double position)
        {
            var curvature_ed = Curvatures[curvatureEditingId];
            // Validate position.
            foreach(var curv in Curvatures)
            {
                if (curv == curvature_ed)
                    continue;

                // Border case is when editable curvature touches another curvature.
                var curv_start = curv.Start + 0.001;
                var curv_end = curv.End - 0.001;

                if (Math.Sign(position - curvatureStart) == Math.Sign(curv_start - curvatureStart))
                    if (Math.Abs(position - curvatureStart) >= Math.Abs(curv_start - curvatureStart))
                        position = curv.Start;

                if (Math.Sign(position - curvatureStart) == Math.Sign(curv_end - curvatureStart))
                    if (Math.Abs(position - curvatureStart) >= Math.Abs(curv_end - curvatureStart))
                        position = curv.End;
            }

            curvature_ed.Start = Math.Min(position, curvatureStart);
            curvature_ed.Length = Math.Abs(position - curvatureStart);

            // Elsewhere ObservableCollection not calls CollectionChanged.
            Curvatures.RemoveAt(curvatureEditingId);
            Curvatures.Insert(curvatureEditingId, curvature_ed);

            trackEditor.Dirtied();
        }

        const double MIN_CURVATURE_LENGTH = 0.1;

        public void FinishCurvatureEdit()
        {
            if (Curvatures[curvatureEditingId].Length < MIN_CURVATURE_LENGTH)
                Curvatures.RemoveAt(curvatureEditingId);
            IsCurvatureEditing = false;
        }

        public void FinishCurvatureCreate()
        {
            if (Curvatures[curvatureEditingId].Length < MIN_CURVATURE_LENGTH)
                Curvatures.RemoveAt(curvatureEditingId);
            IsCurvatureEditing = false;
        }

        public void DeleteCurvatureAt(double position)
        {
            for (int i = 0; i < Curvatures.Count; i++)
            {
                var curvature = Curvatures[i];
                if (curvature.Start <= position && curvature.End >= position)
                {
                    Curvatures.RemoveAt(i);
                    return;
                }  
            }

            trackEditor.Dirtied();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}