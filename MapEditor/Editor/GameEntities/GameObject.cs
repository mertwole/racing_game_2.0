using System.ComponentModel;

namespace Editor.GameEntities
{
    public class GameObject : INotifyPropertyChanged
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

        public GameObject(double road_distance, double offset)
        {
            RoadDistance = road_distance;
            Offset = offset;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
