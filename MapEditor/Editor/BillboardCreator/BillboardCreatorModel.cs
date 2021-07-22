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
            dirty = true;
        }

        public void AddLOD(string file_path)
        {
            var img = (Bitmap)Image.FromFile(file_path);
            billboard.AddLOD(new LOD(img));
            dirty = true;
        }

        public void DeleteLOD(int id)
        {
            billboard.RemoveLOD(id);
            dirty = true;
        }

        FileManager.File loadedFrom = null;
        bool dirty = false;

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new Billboard(billboard);
            dirty = false;
        }

        public void LoadFromFile(FileManager.File file)
        {
            if (!(file.Content is Billboard))
                throw new System.Exception("Unexpected file type. Expected file containing Billboard.");

            billboard = new Billboard(file.Content as Billboard);
            loadedFrom = file;
            dirty = false;
        }
    }
}
