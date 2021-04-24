using System.Collections.ObjectModel;

namespace Editor.GameEntities
{
    public class LOD
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class Billboard
    {
        public ObservableCollection<LOD> LODs { get; private set; }

        public Billboard()
        {
            LODs = new ObservableCollection<LOD>();

            LODs.Add(new LOD());
            LODs[0].Width = 500;
            LODs[0].Height = 200;

            LODs.Add(new LOD());
            LODs[1].Width = 300;
            LODs[1].Height = 100;

            LODs.Add(new LOD());
            LODs[2].Width = 200;
            LODs[2].Height = 100;
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
