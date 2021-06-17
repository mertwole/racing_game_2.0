using Editor.FileManager;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Editor.GameEntities
{
    public class GameObject : SaveableEntity, INotifyPropertyChanged
    {
        ObservableCollection<Billboard> billboards = new ObservableCollection<Billboard>();
        public ObservableCollection<Billboard> Billboards { get => billboards; }

        double roadDistance;
        public double RoadDistance
        {
            get => roadDistance;
            set
            {
                roadDistance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoadDistance"));
            }
        }

        double offset;
        public double Offset
        {
            get => offset;
            set
            {
                offset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Offset"));
            }
        }

        public GameObject()
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

        public GameObject(double road_distance, double offset)
        {
            RoadDistance = road_distance;
            Offset = offset;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileIcon GetIcon()
        {
            return FileIcon.GameObject;
        }
    }
}
