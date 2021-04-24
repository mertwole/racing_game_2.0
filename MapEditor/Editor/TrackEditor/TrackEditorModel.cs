using Editor.TrackEditor.BillboardEditor;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.HeelEditor;

namespace Editor.TrackEditor
{
    public static class TrackEditorModel
    {
        static BillboardEditorModel billboardEditorModel = new BillboardEditorModel();
        public static BillboardEditorModel BillboardEditorModel { get => billboardEditorModel; }

        static CurvatureEditorModel curvatureEditorModel = new CurvatureEditorModel();
        public static CurvatureEditorModel CurvatureEditorModel { get => curvatureEditorModel; }

        static HeelEditorModel heelEditorModel = new HeelEditorModel();
        public static HeelEditorModel HeelEditorModel { get => heelEditorModel; }
    }
}
