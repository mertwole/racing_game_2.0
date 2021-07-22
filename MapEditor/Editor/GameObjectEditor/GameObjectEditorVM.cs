using Editor.CustomControls;
using Editor.GameEntities;
namespace Editor.GameObjectEditor
{
    public class GameObjectEditorVM
    {
        GameObjectEditorModel model = ModelLocator.GetModel<GameObjectEditorModel>();

        public GameObject GameObject { get => model.GameObject; }
    }
}
