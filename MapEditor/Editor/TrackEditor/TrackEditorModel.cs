using Editor.GameEntities;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
using Editor.TrackEditor.TrackPreview;
using System.Drawing;
using System.IO;

namespace Editor.TrackEditor
{
    public class TrackEditorModel : IEditorTabModel
    {
        Track track;
        public Track Track { get => track; }

        CurvatureEditorView curvatureEditorView;
        GameObjectLocationEditorView gameObjectLocationEditorView;
        HeelEditorView heelEditorView;
        TrackPreviewView trackPreviewView;

        TrackPreviewModel trackPreviewModel;

        public CurvatureEditorView CurvatureEditorView { get => curvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => gameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => heelEditorView; }
        public TrackPreviewView TrackPreviewView { get => trackPreviewView; }

        double pointerPositionNormalized = 0.0;
        public double PointerPositionNormalized {
            set
            {
                pointerPositionNormalized = value;
                UpdatePreview();
            }
            get => pointerPositionNormalized; 
        }

        Size previewSize = new Size(192, 108);
        public Size PreviewSize { get => previewSize; }

        public TrackEditorModel(Track track)
        {
            this.track = track;

            track.PropertyChanged += (s, e) =>
            {
                UpdatePreview();
            };

            curvatureEditorView = new CurvatureEditorView();
            var c_vm = curvatureEditorView.DataContext as CurvatureEditorVM;
            c_vm.Model = new CurvatureEditorModel(this);

            gameObjectLocationEditorView = new GameObjectLocationEditorView();
            var go_vm = gameObjectLocationEditorView.DataContext as GameObjectLocationEditorVM;
            go_vm.Model = new GameObjectLocationEditorModel(this);

            heelEditorView = new HeelEditorView();
            var h_vm = heelEditorView.DataContext as HeelEditorVM;
            h_vm.Model = new HeelEditorModel(this);

            trackPreviewView = new TrackPreviewView();
            var tp_vm = trackPreviewView.DataContext as TrackPreviewVM;
            tp_vm.Model = trackPreviewModel = new TrackPreviewModel(previewSize);

            UpdatePreview();
        }

        static Serializers serializer = new Serializers();
        void UpdatePreview()
        {
            var serialized = serializer.SerializeRmapSingleTrack(track);
            serialized.Seek(0, SeekOrigin.Begin);
            var rmap_data = serialized.ToArray();
            trackPreviewModel.Update(rmap_data, (float)pointerPositionNormalized * (float)track.Parameters.Length);
        }
    }
}
