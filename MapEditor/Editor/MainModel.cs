using Editor.FileManager;
using Editor.TabbedEditors;
using Editor.TrackPreview;
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

        static TrackPreviewModel trackPreviewModel = new TrackPreviewModel();
        public static TrackPreviewModel TrackPreviewModel { get => trackPreviewModel; }

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
            var serialized = serializer.SerializeRproj(fileManagerModel.Hierarchy.ToList());
            serialized.Seek(0, SeekOrigin.Begin);

            var fs = File.OpenWrite(projFilePath);
            serialized.WriteTo(fs);
            fs.Close();
        }

        public static void TESTTT()
        {
            var serialized = serializer.SerializeRmap(fileManagerModel.Hierarchy.ToList());
            serialized.Seek(0, SeekOrigin.Begin);
            var rmap_data = serialized.ToArray();
            trackPreviewModel.TrackDataChanged(rmap_data);
        }

        public static void LoadProject()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".rproj";
            ofd.Filter = "project files (*.rproj)|*.rproj";
            if (ofd.ShowDialog() != true)
                return;

            projFilePath = ofd.FileName;

            var fs = File.OpenRead(projFilePath);
            var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var hierarchy = new Serializers().DeserializeRproj(ms);
            fs.Close();
            ms.Close();

            bool all_tabs_closed = tabbedEditorsModel.CloseAllTabs();
            if (!all_tabs_closed)
                return;

            FileManagerModel.ReplaceHierarchy(hierarchy);
        }
    }
}
