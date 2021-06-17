using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.GameObjectEditor.BillboardList
{
    public class BillboardListVM
    {
        BillboardListModel model = ModelLocator.GetModel<BillboardListModel>();

        public ObservableCollection<Billboard> Billboards { get => model.Billboards; }
    }
}
