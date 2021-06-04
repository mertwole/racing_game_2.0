using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Editor.GameObjectEditor.BillboardList
{
    public class BillboardListModel
    {
        ObservableCollection<Billboard> billboards = new ObservableCollection<Billboard>();
        public ObservableCollection<Billboard> Billboards { get => billboards; }

        public BillboardListModel()
        {
            Bitmap bmp = new Bitmap(200, 100);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 200, 50));

            var bb = new Billboard();
            bb.AddLOD(new LOD(bmp));

            billboards.Add(new Billboard(bb));
            billboards.Add(new Billboard(bb));
            billboards.Add(new Billboard(bb));
        }
    }
}
