using Editor.FileManager;
using System.ComponentModel;

namespace Editor.TrackEditor
{
    public class TrackEditorModel : IEditorTabModel
    {
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
