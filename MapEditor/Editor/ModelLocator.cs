using Editor.FileManager;
using Editor.TabbedEditors;
using Editor.TrackEditor;
using System;
using System.Collections.Generic;

namespace Editor
{
    // Creates and initializes all the models in app
    // also constructing hierarchy of them
    // then VMs can get them by type.
    static class ModelLocator
    {
        static Dictionary<Type, object> models = new Dictionary<Type, object>();

        static ModelLocator()
        {
            //models.Add(typeof(TrackEditorModel), new TrackEditorModel());

            models.Add(typeof(FileManagerModel), new FileManagerModel());

            models.Add(typeof(TabbedEditorsModel), new TabbedEditorsModel());
        }

        public static T GetModel<T>()
        {
            if (models.ContainsKey(typeof(T)))
                return (T)models[typeof(T)];

            throw new Exception("model not found");
        }
    }
}
