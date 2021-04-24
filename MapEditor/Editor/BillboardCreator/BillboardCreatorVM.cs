using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.BillboardCreator
{
    public class BillboardCreatorVM
    {
        BillboardCreatorModel model = new BillboardCreatorModel();

        public ObservableCollection<LOD> LODs { get => model.LODs; }
    }
}
