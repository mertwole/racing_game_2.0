using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Editor.BillboardCreator
{
    public class BillboardCreatorModel
    {
        Billboard billboard = new Billboard();
        public ObservableCollection<LOD> LODs { get => billboard.LODs; }

        public void MoveLODTo(int lod_id, int move_to)
        {
            billboard.LODs.Move(lod_id, move_to);
        }

        public void AddLOD(string file_path)
        {
            var img = (Bitmap)Bitmap.FromFile(file_path);
            billboard.AddLOD(new LOD(img));
        }
    }
}
