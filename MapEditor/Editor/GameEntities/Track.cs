﻿using Editor.FileManager;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Editor.GameEntities
{
    public class Track : ISaveableEntity
    {
        ObservableCollection<HeelKeypoint> keypoints = new ObservableCollection<HeelKeypoint>();
        public ObservableCollection<HeelKeypoint> Keypoints { get => keypoints; }

        ObservableCollection<Curvature> curvatures = new ObservableCollection<Curvature>();
        public ObservableCollection<Curvature> Curvatures { get => curvatures; }

        ObservableCollection<GameObject> gameObjects = new ObservableCollection<GameObject>();
        public ObservableCollection<GameObject> GameObjects { get => gameObjects; }

        double length = 100.0;
        public double Length { get => length; set => length = value; }

        Color mainColor = Color.FromArgb(255, 100, 100, 100);
        public Color MainColor { get => mainColor; set => mainColor = value; }
        Color secondaryColor = Color.FromArgb(255, 150, 150, 150);
        public Color SecondaryColor { get => secondaryColor; set => secondaryColor = value; }
             
        public Track()
        {

        }

        public Track(Track prototype)
        {
            foreach (var keypoint in prototype.keypoints)
                keypoints.Add(new HeelKeypoint(keypoint));

            foreach (var curvature in prototype.curvatures)
                curvatures.Add(curvature);

            foreach (var game_object in prototype.gameObjects)
                gameObjects.Add(game_object);

            mainColor = prototype.mainColor;
            secondaryColor = prototype.secondaryColor;
            length = prototype.length;
        }

        public FileIcon GetIcon()
        {
            return FileIcon.Track;
        }
    }
}
