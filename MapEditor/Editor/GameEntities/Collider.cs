using System.ComponentModel;

namespace Editor.GameEntities
{
    public class Collider : INotifyPropertyChanged
    {
        Vector3 size;
        public Vector3 Size
        {
            get => size;
            set
            {
                size = value;
                OnPropertyChanged("Size");
                OnPropertyChanged("SizeX");
                OnPropertyChanged("SizeY");
                OnPropertyChanged("SizeZ");
            }
        }

        Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged("Position");
                OnPropertyChanged("X");
                OnPropertyChanged("Y");
                OnPropertyChanged("Z");
            }
        }

        public Collider(Vector3 position, Vector3 size)
        {
            this.position = position;
            this.size = size;
        }

        public Collider(Collider prototype)
        {
            position = prototype.position;
            size = prototype.size;
        }

        // Bacause Two-way binding not works for structs.
        public double X { get => position.X; set { position.X = value; OnPropertyChanged("X"); OnPropertyChanged("Position"); } }
        public double Y { get => position.Y; set { position.Y = value; OnPropertyChanged("Y"); OnPropertyChanged("Position"); } }
        public double Z { get => position.Z; set { position.Z = value; OnPropertyChanged("Z"); OnPropertyChanged("Position"); } }

        // Bacause Two-way binding not works for structs.
        public double SizeX { get => size.X; set { size.X = value; OnPropertyChanged("SizeX"); OnPropertyChanged("Size"); } }
        public double SizeY { get => size.Y; set { size.Y = value; OnPropertyChanged("SizeY"); OnPropertyChanged("Size"); } }
        public double SizeZ { get => size.Z; set { size.Z = value; OnPropertyChanged("SizeZ"); OnPropertyChanged("Size"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
