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
