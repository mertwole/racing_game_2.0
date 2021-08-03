using System.ComponentModel;

namespace Editor
{
    public interface IEditorTabModel : INotifyPropertyChanged
    {
        bool IsDirty { get; }
        void ApplyChanges();
    }
}
