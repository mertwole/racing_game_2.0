using Editor.FileManager;
using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorModel : INotifyPropertyChanged, IEditorTabModel
    {
        GameObject gameObject; 
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        FileManager.File loadedFrom = null;
        bool dirty = false;
        public bool IsDirty => dirty;

        public GameObjectEditorModel(FileManager.File file)
        {
            if (!(file.Content is GameObject))
                throw new System.Exception("Unexpected file type. Expected file containing GameObject.");

            gameObject = new GameObject(file.Content as GameObject);
            loadedFrom = file;
            dirty = false;

            OnPropertyChanged("IsDirty");
            OnPropertyChanged("GameObject");
        }

        object toMove = null;

        public void StartMoveObject(object to_move)
        {
            toMove = to_move;
        }

        public void MoveObject(Vector3 new_pos)
        {
            if(toMove is Collider collider)
                collider.Position = new_pos;
            else
            {
                var billboard = toMove as PositionedBillboard;
                billboard.Position = new_pos;
            }

            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void DeleteObject(object obj)
        {
            if (obj is PositionedBillboard bb && gameObject.Billboards.Contains(bb))
                gameObject.Billboards.Remove(bb);
            else if (gameObject.Colliders.Contains(obj as Collider))
                gameObject.Colliders.Remove(obj as Collider);

            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void AddCollider()
        {
            var collider = new Collider(new Vector3(), new Vector3(1, 1, 1));
            gameObject.Colliders.Add(collider);
            dirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void AddBillboardFromFile(File file)
        {
            if(file.Content is Billboard billboard)
            {
                gameObject.Billboards.Add(new PositionedBillboard(billboard));

                dirty = true;
                OnPropertyChanged("IsDirty");
            }
        }

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new GameObject(gameObject);
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
