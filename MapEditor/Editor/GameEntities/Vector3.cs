using System.ComponentModel;

namespace Editor.GameEntities
{
    public struct Vector3 : INotifyPropertyChanged
    {
        double x, y, z;

        public double X { get => x; set { x = value; OnPropertyChanged("X"); } }
        public double Y { get => y; set { y = value; OnPropertyChanged("Y"); } }
        public double Z { get => z; set { z = value; OnPropertyChanged("Z"); } }

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            PropertyChanged = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
