using Editor.FileManager;
using System.ComponentModel;
using System.Drawing;

namespace Editor.GameEntities
{
    public class LOD
    {
        public double Width { get; private set; }
        public double Height { get; private set; }

        Bitmap image;
        public Bitmap Image { get => image; }

        public LOD(Bitmap image)
        {
            this.image = image;
            Width = image.Width;
            Height = image.Height;
        }
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

    public class Billboard : ISaveableEntity, INotifyPropertyChanged
    {
        public BindingList<LOD> LODs { get; private set; }
        public Bitmap Preview { get => LODs[0].Image; }

        public Billboard()
        {
            LODs = new BindingList<LOD>();
            UpdateOnPropertyChanged();
        }

        public Billboard(Billboard prototype)
        {
            LODs = new BindingList<LOD>(prototype.LODs);
            UpdateOnPropertyChanged();
        }

        public void AddLOD(LOD lod)
        {
            LODs.Add(lod);
        }

        public void RemoveLOD(int id)
        {
            if(id >= 0 && id < LODs.Count) 
                LODs.RemoveAt(id);
        }

        void UpdateOnPropertyChanged()
        {
            LODs.ListChanged += (s, e) =>
            {
                OnPropertyChanged("LODs");
                OnPropertyChanged("Preview");
            };
        }

        public ISaveableEntity Clone()
        {
            return new Billboard(this);
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Billboard;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
