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
    }
}
