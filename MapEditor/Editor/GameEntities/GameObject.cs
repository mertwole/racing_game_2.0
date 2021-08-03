using Editor.FileManager;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Editor.GameEntities
{
    public class GameObject : ISaveableEntity, INotifyPropertyChanged
    {
        ObservableCollection<Billboard> billboards = new ObservableCollection<Billboard>();
        public ObservableCollection<Billboard> Billboards { get => billboards; }

        ObservableCollection<Collider> colliders = new ObservableCollection<Collider>();
        public ObservableCollection<Collider> Colliders { get => colliders; }

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
            bb.Width = 4;
            bb.X = 0;
            bb.Z = -1;

            bmp = new Bitmap(200, 100);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 50, 200));

            var bb2 = new Billboard();
            bb2.AddLOD(new LOD(bmp));
            bb2.Width = 1;
            bb2.X = 2;
            bb2.Z = 0;

            billboards.Add(bb);
            billboards.Add(bb2);

            var collider = new Collider(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            colliders.Add(collider);
        }

        public GameObject(GameObject prototype)
        {
            roadDistance = prototype.roadDistance;
            offset = prototype.offset;

            billboards = new ObservableCollection<Billboard>();
            foreach (var billboard in prototype.billboards)
                billboards.Add(new Billboard(billboard));

            colliders = new ObservableCollection<Collider>();
            foreach (var collider in prototype.colliders)
                colliders.Add(new Collider(collider));

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
