using Editor.FileManager;
using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorModel : INotifyPropertyChanged
    {
        GameObject gameObject; 
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        public GameObjectEditorModel(GameObject gameObject)
        {
            this.gameObject = gameObject;
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
                var billboard = toMove as Billboard;
                billboard.Position = new_pos;
            }
        }

        public void DeleteObject(object obj)
        {
            if (obj is Billboard bb && gameObject.Billboards.Contains(bb))
                gameObject.Billboards.Remove(bb);
            else if (gameObject.Colliders.Contains(obj as Collider))
                gameObject.Colliders.Remove(obj as Collider);
        }

        public void AddCollider()
        {
            var collider = new Collider(new Vector3(), new Vector3(1, 1, 1));
            gameObject.Colliders.Add(collider);
        }

        public void AddBillboardFromFile(File file)
        {
            if(file.Content is Billboard billboard)
                gameObject.Billboards.Add(new Billboard(billboard));
        }

        FileManager.File loadedFrom = null;
        bool dirty = false;

        public void LoadFromFile(FileManager.File file)
        {
            if (!(file.Content is GameObject))
                throw new System.Exception("Unexpected file type. Expected file containing GameObject.");

            gameObject = new GameObject(file.Content as GameObject);
            loadedFrom = file;
            dirty = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameObject"));
        }

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new GameObject(gameObject);
            dirty = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
