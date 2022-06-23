using Editor.FileManager;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.GameEntities
{
    public class PositionedGameObject : INotifyPropertyChanged
    {
        GameObject gameObject;
        public GameObject GameObject { get => gameObject; }

        double roadDistance;
        public double RoadDistance
        {
            get => roadDistance;
            set
            {
                roadDistance = value;
                OnPropertyChanged("RoadDistance");
            }
        }

        double offset;
        public double Offset
        {
            get => offset;
            set
            {
                offset = value;
                OnPropertyChanged("Offset");
            }
        }

        /// Cloning game_object
        public PositionedGameObject(GameObject game_object)
        {
            gameObject = new GameObject(game_object);
            gameObject.PropertyChanged += (s, e) => OnPropertyChanged("GameObject");
        }

        public PositionedGameObject(PositionedGameObject prototype)
        {
            gameObject = new GameObject(prototype.gameObject);
            gameObject.PropertyChanged += (s, e) => OnPropertyChanged("GameObject");

            roadDistance = prototype.roadDistance;
            offset = prototype.offset;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameObject : ISaveableEntity, INotifyPropertyChanged
    {
        ObservableCollection<Billboard> billboards = new ObservableCollection<Billboard>();
        public ObservableCollection<Billboard> Billboards { get => billboards; }

        ObservableCollection<Collider> colliders = new ObservableCollection<Collider>();
        public ObservableCollection<Collider> Colliders { get => colliders; }

        public GameObject()
        {
            billboards.CollectionChanged += (s, e) => OnPropertyChanged("Billboards");
            colliders.CollectionChanged += (s, e) => OnPropertyChanged("Colliders");
        }

        public GameObject(GameObject prototype)
        {
            billboards = new ObservableCollection<Billboard>();
            foreach (var billboard in prototype.billboards)
                billboards.Add(new Billboard(billboard));

            billboards.CollectionChanged += (s, e) => OnPropertyChanged("Billboards");

            colliders = new ObservableCollection<Collider>();
            foreach (var collider in prototype.colliders)
                colliders.Add(new Collider(collider));

            colliders.CollectionChanged += (s, e) => OnPropertyChanged("Colliders");
        }

        public FileIcon GetIcon()
        {
            return FileIcon.GameObject;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
