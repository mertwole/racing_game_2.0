using Editor.FileManager;
using System.ComponentModel;

namespace Editor.GameEntities
{
    public interface ISaveableEntity : INotifyPropertyChanged
    {
        FileIcon GetIcon();
        ISaveableEntity Clone();
    }
}
