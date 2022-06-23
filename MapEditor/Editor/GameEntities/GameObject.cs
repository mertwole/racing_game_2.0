﻿using Editor.FileManager;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.GameEntities
{
    public class Collider : INotifyPropertyChanged
    {
        Vector3 size;
        public Vector3 Size
        {
            get => size;
            set
            {
                size = value;
                OnPropertyChanged("Size");
                OnPropertyChanged("SizeX");
                OnPropertyChanged("SizeY");
                OnPropertyChanged("SizeZ");
            }
        }

        Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged("Position");
                OnPropertyChanged("X");
                OnPropertyChanged("Y");
                OnPropertyChanged("Z");
            }
        }

        public Collider(Vector3 position, Vector3 size)
        {
            this.position = position;
            this.size = size;
        }

        public Collider(Collider prototype)
        {
            position = prototype.position;
            size = prototype.size;
        }

        // Bacause Two-way binding not works for structs.
        public double X { get => position.X; set { position.X = value; OnPropertyChanged("X"); OnPropertyChanged("Position"); } }
        public double Y { get => position.Y; set { position.Y = value; OnPropertyChanged("Y"); OnPropertyChanged("Position"); } }
        public double Z { get => position.Z; set { position.Z = value; OnPropertyChanged("Z"); OnPropertyChanged("Position"); } }

        // Bacause Two-way binding not works for structs.
        public double SizeX { get => size.X; set { size.X = value; OnPropertyChanged("SizeX"); OnPropertyChanged("Size"); } }
        public double SizeY { get => size.Y; set { size.Y = value; OnPropertyChanged("SizeY"); OnPropertyChanged("Size"); } }
        public double SizeZ { get => size.Z; set { size.Z = value; OnPropertyChanged("SizeZ"); OnPropertyChanged("Size"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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
        ObservableCollection<PositionedBillboard> billboards = new ObservableCollection<PositionedBillboard>();
        public ObservableCollection<PositionedBillboard> Billboards { get => billboards; }

        ObservableCollection<Collider> colliders = new ObservableCollection<Collider>();
        public ObservableCollection<Collider> Colliders { get => colliders; }

        public GameObject()
        {
            billboards.CollectionChanged += (s, e) => OnPropertyChanged("Billboards");
            colliders.CollectionChanged += (s, e) => OnPropertyChanged("Colliders");
        }

        public GameObject(GameObject prototype)
        {
            billboards = new ObservableCollection<PositionedBillboard>();
            foreach (var billboard in prototype.billboards)
                billboards.Add(new PositionedBillboard(billboard));

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
