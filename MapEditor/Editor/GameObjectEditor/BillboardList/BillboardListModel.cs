using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.GameObjectEditor.BillboardList
{
    public class BillboardListModel
    {
        public ObservableCollection<Billboard> Billboards { get => gameObject.Billboards; }
        GameObject gameObject;

        public BillboardListModel(GameObjectEditorModel gameObjectEditorModel)
        {
            gameObject = gameObjectEditorModel.GameObject;
        }
    }
}
