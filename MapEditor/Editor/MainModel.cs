using Editor.FileManager;
using Editor.TabbedEditors;

namespace Editor
{
    public static class MainModel
    {
        static FileManagerModel fileManagerModel = new FileManagerModel();
        public static FileManagerModel FileManagerModel { get => fileManagerModel; }

        static TabbedEditorsModel tabbedEditorsModel = new TabbedEditorsModel();
        public static TabbedEditorsModel TabbedEditorsModel { get => tabbedEditorsModel; }
    }
}
