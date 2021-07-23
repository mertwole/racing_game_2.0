﻿using Editor.GameEntities;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorModel
    {
        GameObject gameObject; 
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        public GameObjectEditorModel(GameObject gameObject)
        {
            this.gameObject = gameObject;
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
        }

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new GameObject(gameObject);
            dirty = false;
        }
    }
}