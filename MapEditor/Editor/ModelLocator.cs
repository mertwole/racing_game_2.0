using Editor.BillboardCreator;
using Editor.FileManager;
using Editor.GameEntities;
using Editor.GameObjectEditor;
using Editor.TabbedEditors;
using Editor.TrackEditor;
using Editor.TrackEditor.BillboardEditor;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.HeelEditor;
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
            GameObjectEditorModel gameObjectEditorModel = new GameObjectEditorModel(new GameObject());

            models.Add(typeof(GameObjectEditorModel), gameObjectEditorModel);

            models.Add(typeof(BillboardEditorModel), new BillboardEditorModel());
            models.Add(typeof(CurvatureEditorModel), new CurvatureEditorModel());
            models.Add(typeof(HeelEditorModel), new HeelEditorModel());
            models.Add(typeof(TrackEditorModel), new TrackEditorModel());

            models.Add(typeof(BillboardCreatorModel), new BillboardCreatorModel());

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
