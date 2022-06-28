using Editor.FileManager;
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

    public class PositionedBillboard : INotifyPropertyChanged
    {
        Billboard billboard;
        public Billboard Billboard { get => billboard; }

        Vector3 position = new Vector3(0, 0, 0);
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

        // Bacause Two-way binding not works for structs.
        public double X { get => position.X; set { position.X = value; OnPropertyChanged("X"); OnPropertyChanged("Position"); } }
        public double Y { get => position.Y; set { position.Y = value; OnPropertyChanged("Y"); OnPropertyChanged("Position"); } }
        public double Z { get => position.Z; set { position.Z = value; OnPropertyChanged("Z"); OnPropertyChanged("Position"); } }

        double width = 1;
        public double Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged("Width");
                OnPropertyChanged("Height");
            }
        }

        public double Height { get => width / billboard.Preview.Width * billboard.Preview.Height; }

        /// Clones billboard
        public PositionedBillboard(Billboard billboard)
        {
            this.billboard = new Billboard(billboard);
            billboard.PropertyChanged += (s, e) => OnPropertyChanged("Billboard");
        }

        public PositionedBillboard(PositionedBillboard prototype)
        {
            billboard = new Billboard(prototype.billboard);
            width = prototype.width;
            position = prototype.position;

            billboard.PropertyChanged += (s, e) => OnPropertyChanged("Billboard");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameObject : ISaveableEntity, INotifyPropertyChanged
    {
        BindingList<PositionedBillboard> billboards = new BindingList<PositionedBillboard>();
        public BindingList<PositionedBillboard> Billboards { get => billboards; }

        BindingList<Collider> colliders = new BindingList<Collider>();
        public BindingList<Collider> Colliders { get => colliders; }

        public GameObject()
        {
            UpdateOnPropertyChaned();
        }

        public GameObject(GameObject prototype)
        {
            billboards = new BindingList<PositionedBillboard>();
            foreach (var billboard in prototype.billboards)
                billboards.Add(new PositionedBillboard(billboard));

            colliders = new BindingList<Collider>();
            foreach (var collider in prototype.colliders)
                colliders.Add(new Collider(collider));

            UpdateOnPropertyChaned();
        }

        void UpdateOnPropertyChaned()
        {
            billboards.ListChanged += (s, e) => OnPropertyChanged("Billboards");
            colliders.ListChanged += (s, e) => OnPropertyChanged("Colliders");
        }

        public ISaveableEntity Clone()
        {
            return new GameObject(this);
        }

        public FileIcon GetIcon()
        {
            return FileIcon.GameObject;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
