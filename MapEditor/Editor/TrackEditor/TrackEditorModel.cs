using Editor.GameEntities;
using Editor.TrackEditor.CurvatureEditor;
using Editor.TrackEditor.GameObjectLocationEditor;
using Editor.TrackEditor.HeelEditor;
using System.ComponentModel;

namespace Editor.TrackEditor
{
    public class TrackEditorModel : IEditorTabModel
    {
        Track track;
        public Track Track { get => track; }

        CurvatureEditorView curvatureEditorView;
        GameObjectLocationEditorView gameObjectLocationEditorView;
        HeelEditorView heelEditorView;

        public CurvatureEditorView CurvatureEditorView { get => curvatureEditorView; }
        public GameObjectLocationEditorView GameObjectLocationEditorView { get => gameObjectLocationEditorView; }
        public HeelEditorView HeelEditorView { get => heelEditorView; }

        double trackLength = 120;
        public double TrackLength { get => trackLength; set => trackLength = value; }

        public TrackEditorModel(FileManager.File file)
        {
            if (!(file.Content is Track))
                throw new System.Exception("Unexpected file type. Expected file containing Track.");

            loadedFrom = file;
            track = new Track(file.Content as Track);

            curvatureEditorView = new CurvatureEditorView();
            var c_vm = curvatureEditorView.DataContext as CurvatureEditorVM;
            c_vm.Model = new CurvatureEditorModel(this);

            gameObjectLocationEditorView = new GameObjectLocationEditorView();
            var go_vm = gameObjectLocationEditorView.DataContext as GameObjectLocationEditorVM;
            go_vm.Model = new GameObjectLocationEditorModel(this);

            heelEditorView = new HeelEditorView();
            var h_vm = heelEditorView.DataContext as HeelEditorVM;
            h_vm.Model = new HeelEditorModel(this);
        }

        FileManager.File loadedFrom;

        bool isDirty = false;
        public bool IsDirty => isDirty;

        public void Dirtied()
        {
            isDirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void ApplyChanges()
        {
            if (loadedFrom == null) return;

            loadedFrom.Content = new Track(track);

            isDirty = false;
            OnPropertyChanged("IsDirty");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
