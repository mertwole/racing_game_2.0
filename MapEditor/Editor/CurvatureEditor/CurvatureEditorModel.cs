using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.CurvatureEditor
{
    public class Curvature
    {
        public const double MIN_LENGTH = 10.0;

        public double Start { get; set; }
        public double Length { get; set; }
        public double End { get => Start + Length; }
        public double Value { get; set; }

        public Curvature(double start, double length, double value)
        {
            Start = start;
            Length = length;
            Value = value;
        }
    }

    public class CurvatureEditorModel : INotifyPropertyChanged
    {
        ObservableCollection<Curvature> curvatures = new ObservableCollection<Curvature>();
        public ObservableCollection<Curvature> Curvatures { get => curvatures; }
        bool isCurvatureEditing = false;
        public bool IsCurvatureEditing { 
            get => isCurvatureEditing; 
            private set { isCurvatureEditing = value; OnPropertyChanged("IsCurvatureEditing"); } 
        }

        public void CreateCurvature(double position)
        {
            foreach (var curv in curvatures)
                if (curv.End > position && curv.Start < position)
                    return;

            IsCurvatureEditing = true;
            curvatureStart = position;
            curvatures.Add(new Curvature(position, 0.0, 0.1));
            curvatureEditingId = curvatures.Count - 1;
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

            for(int i = 0; i < curvatures.Count; i++)
            {
                var curvature = curvatures[i];

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
            var curvature_ed = curvatures[curvatureEditingId];
            // Validate position.
            foreach(var curv in curvatures)
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
            curvatures.RemoveAt(curvatureEditingId);
            curvatures.Insert(curvatureEditingId, curvature_ed);
        }

        public void FinishCurvatureEdit()
        {
            if (curvatures[curvatureEditingId].Length < Curvature.MIN_LENGTH)
                curvatures.RemoveAt(curvatureEditingId);
            IsCurvatureEditing = false;
        }

        public void FinishCurvatureCreate()
        {
            if (curvatures[curvatureEditingId].Length < Curvature.MIN_LENGTH)
                curvatures.RemoveAt(curvatureEditingId);
            IsCurvatureEditing = false;
        }

        public void DeleteCurvatureAt(double position)
        {
            for(int i = 0; i < curvatures.Count; i++)
            {
                var curvature = curvatures[i];
                if (curvature.Start <= position && curvature.End >= position)
                {
                    curvatures.RemoveAt(i);
                    return;
                }  
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}