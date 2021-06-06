using Editor.FileManager;
using System.ComponentModel;

namespace Editor.GameEntities
{
    public class GameObject : SaveableEntity, INotifyPropertyChanged
    {
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
