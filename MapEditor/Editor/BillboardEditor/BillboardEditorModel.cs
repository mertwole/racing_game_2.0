using Editor.GameEntities;
using System.ComponentModel;
using System.Drawing;

namespace Editor.BillboardEditor
{
    public class BillboardEditorModel : IEditorTabModel
    {
        Billboard billboard;
        public BindingList<LOD> LODs { get => billboard.LODs; }

        public BillboardEditorModel(Billboard billboard)
        {
            this.billboard = billboard;
        }

        public void MoveLODTo(int lod_id, int move_to)
        {
            var lod = billboard.LODs[lod_id];
            billboard.LODs.RemoveAt(lod_id);
            billboard.LODs.Insert(move_to, lod);
        }

        public void AddLOD(string file_path)
        {
            var img = (Bitmap)Image.FromFile(file_path);
            billboard.AddLOD(new LOD(img));
        }

        public void DeleteLOD(int id)
        {
            billboard.RemoveLOD(id);
        }
    }
}
