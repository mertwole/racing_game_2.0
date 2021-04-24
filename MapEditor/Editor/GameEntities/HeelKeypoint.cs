using System.ComponentModel;

namespace Editor.GameEntities
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
}
