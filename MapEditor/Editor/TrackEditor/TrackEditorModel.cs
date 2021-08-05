using Editor.FileManager;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
using System.ComponentModel;

namespace Editor.TrackEditor
{
    public class TrackEditorModel : IEditorTabModel
    {
        CurvatureEditorView curvatureEditorView;
        GameObjectLocationEditorView gameObjectLocationEditorView;
        HeelEditorView heelEditorView;

        public CurvatureEditorView CurvatureEditorView { get => curvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => gameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => heelEditorView; }

        public TrackEditorModel()
        {
            curvatureEditorView = new CurvatureEditorView();
            var c_vm = (curvatureEditorView.DataContext) as CurvatureEditorVM;
            c_vm.Model = new CurvatureEditorModel();

            gameObjectLocationEditorView = new GameObjectLocationEditorView();
            var go_vm = (gameObjectLocationEditorView.DataContext) as GameObjectLocationEditorVM;
            go_vm.Model = new GameObjectLocationEditorModel();

            heelEditorView = new HeelEditorView();
            var h_vm = (heelEditorView.DataContext) as HeelEditorVM;
            h_vm.Model = new HeelEditorModel();
        }

        public bool IsDirty => false;

        public void ApplyChanges()
        {
            
        }

        public void LoadFromFile(File file)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
