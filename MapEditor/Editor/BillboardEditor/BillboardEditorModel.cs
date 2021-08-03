using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Editor.BillboardEditor
{
    public class BillboardEditorModel : INotifyPropertyChanged, IEditorTabModel
    {
        Billboard billboard;
        public ObservableCollection<LOD> LODs { get => billboard.LODs; }

        public BillboardEditorModel(Billboard billboard)
        {
            this.billboard = billboard;
        }

        public void MoveLODTo(int lod_id, int move_to)
        {
            billboard.LODs.Move(lod_id, move_to);

            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void AddLOD(string file_path)
        {
            var img = (Bitmap)Image.FromFile(file_path);
            billboard.AddLOD(new LOD(img));

            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void DeleteLOD(int id)
        {
            billboard.RemoveLOD(id);

            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        FileManager.File loadedFrom = null;
        bool dirty = false;
        public bool IsDirty => dirty;

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new Billboard(billboard);

            dirty = false;
            OnPropertyChanged("IsDirty");
        }

        public void LoadFromFile(FileManager.File file)
        {
            if (!(file.Content is Billboard))
                throw new System.Exception("Unexpected file type. Expected file containing Billboard.");

            billboard = new Billboard(file.Content as Billboard);
            loadedFrom = file;

            dirty = false;
            OnPropertyChanged("IsDirty");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
