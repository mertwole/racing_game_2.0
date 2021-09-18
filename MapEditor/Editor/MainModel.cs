using Editor.FileManager;
using Editor.TabbedEditors;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Editor
{
    public static class MainModel
    {
        static FileManagerModel fileManagerModel = new FileManagerModel();
        public static FileManagerModel FileManagerModel { get => fileManagerModel; }

        static TabbedEditorsModel tabbedEditorsModel = new TabbedEditorsModel();
        public static TabbedEditorsModel TabbedEditorsModel { get => tabbedEditorsModel; }

        public static bool CanSaveProject { get => projFilePath != null; }
        static string projFilePath = null;

        static Serializers serializer = new Serializers();

        public static void TryEnableSaveProject()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".rproj";
            if (sfd.ShowDialog() == true)
                projFilePath = sfd.FileName;
        }

        public static void SaveProject()
        {
            if (!CanSaveProject) return;
            var serialized = serializer.SerializeRmap(fileManagerModel.Hierarchy.ToList());
            serialized.Seek(0, SeekOrigin.Begin);
            var file_stream = File.Create(projFilePath);
            serialized.CopyTo(file_stream);
            file_stream.Close();
        }
    }
}
