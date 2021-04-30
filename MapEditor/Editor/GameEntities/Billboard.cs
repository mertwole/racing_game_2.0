using System.Collections.ObjectModel;
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

    public class Billboard
    {
        public ObservableCollection<LOD> LODs { get; private set; }

        Vector3 position = new Vector3(0, 0, 0);
        public Vector3 Position { get => position; }

        float width = 1;
        public float Width { get => width; }

        public Billboard()
        {
            LODs = new ObservableCollection<LOD>();
        }

        public Billboard(Billboard prototype)
        {
            LODs = prototype.LODs;
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
    }
}
