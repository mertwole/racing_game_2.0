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
