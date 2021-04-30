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
            var bb = new Billboard();

            Bitmap bmp = new Bitmap(300, 100);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 200, 50));

            bb.AddLOD(new LOD(bmp));

            billboards.Add(bb);
            billboards.Add(bb);
            billboards.Add(bb);
        }
    }
}
