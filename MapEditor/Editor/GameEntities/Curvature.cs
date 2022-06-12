using System.ComponentModel;

namespace Editor.GameEntities
{
    public class Curvature : INotifyPropertyChanged
    {
        double start;
        public double Start { 
            get => start; 
            set { 
                start = value; 
                OnPropertyChanged("Start"); 
                OnPropertyChanged("End"); 
            } 
        }
        double length;
        public double Length { 
            get => length; 
            set { 
                length = value; 
                OnPropertyChanged("Length"); 
                OnPropertyChanged("End"); 
            } 
        }
        public double End { get => Start + Length; }

        double value;
        public double Value { 
            get => value; 
            set 
            { 
                this.value = value;
                OnPropertyChanged("Value");
            } 
        }

        public Curvature(double start, double length, double value)
        {
            Start = start;
            Length = length;
            Value = value;
        }

        public Curvature(Curvature prototype)
        {
            Start = prototype.Start;
            Length = prototype.Length;
            Value = prototype.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
