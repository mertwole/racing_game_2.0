﻿using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorModel : INotifyPropertyChanged
    {
        GameObject gameObject; 
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        public GameObjectEditorModel(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        object toMove = null;

        public void StartMoveObject(object to_move)
        {
            toMove = to_move;
        }

        public void MoveObject(Vector3 new_pos)
        {
            if(toMove is Collider collider)
                collider.Position = new_pos;
            else
            {
                var billboard = toMove as Billboard;
                billboard.Position = new_pos;
            }
        }

        FileManager.File loadedFrom = null;
        bool dirty = false;

        public void LoadFromFile(FileManager.File file)
        {
            if (!(file.Content is GameObject))
                throw new System.Exception("Unexpected file type. Expected file containing GameObject.");

            gameObject = new GameObject(file.Content as GameObject);
            loadedFrom = file;
            dirty = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameObject"));
        }

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new GameObject(gameObject);
            dirty = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
