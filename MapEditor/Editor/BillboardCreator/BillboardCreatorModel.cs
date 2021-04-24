using Editor.GameEntities;
using System.Collections.ObjectModel;

namespace Editor.BillboardCreator
{
    public class BillboardCreatorModel
    {
        Billboard billboard = new Billboard();
        public ObservableCollection<LOD> LODs { get => billboard.LODs; }
    }
}
