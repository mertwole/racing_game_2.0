using Editor.FileManager;
using Editor.GameEntities;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorModel : IEditorTabModel
    {
        GameObject gameObject; 
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        public GameObjectEditorModel(GameObject game_object)
        {
            gameObject = game_object;
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
        }

        public void DeleteObject(object obj)
        {
            if (obj is PositionedBillboard bb && gameObject.Billboards.Contains(bb))
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
            {
                gameObject.Billboards.Add(new PositionedBillboard(billboard));
            }
        }
    }
}
