using Editor.FileManager;
using System.Collections.ObjectModel;
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

    public class Billboard : SaveableEntity, INotifyPropertyChanged
    {
        public ObservableCollection<LOD> LODs { get; private set; }

        Vector3 position = new Vector3(0, 0, 0);
        public Vector3 Position { 
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

        public Bitmap Preview { get => LODs[0].Image; }

        double width = 1;
        public double Width { 
            get => width;
            set 
            { 
                width = value; 
                OnPropertyChanged("Width");
                OnPropertyChanged("Height");
            }
        }

        public double Height { get => width / (double)Preview.Width * (double)Preview.Height; }

        public Billboard()
        {
            LODs = new ObservableCollection<LOD>();
        }

        public Billboard(Billboard prototype)
        {
            LODs = new ObservableCollection<LOD>(prototype.LODs);
            position = prototype.position;
            width = prototype.width;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Billboard;
        }
    }
}
