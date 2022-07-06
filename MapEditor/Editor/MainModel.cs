using Editor.FileManager;
using Editor.TabbedEditors;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Editor
{
    public class MainModel
    {
        public TabbedEditorsModel tabbedEditorsModel = null;
        public FileManagerModel fileManagerModel = null;

        string projFilePath = null;

        static Serializers serializer = new Serializers();

        public void OpenFileEditor(FileManager.File file)
        {
            tabbedEditorsModel.OpenFileEditor(file);
        }

        public void TryEnableSaveProject()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".rproj";
            if (sfd.ShowDialog() == true)
                projFilePath = sfd.FileName;
        }

        public void SaveProject()
        {
            if (projFilePath == null)
            {
                TryEnableSaveProject();
                if (projFilePath == null)
                {
                    var modal = new ProjectSaveFailed();
                    modal.ShowDialog();
                    return;
                }
            }

            var serialized = serializer.SerializeRproj(fileManagerModel.Hierarchy.ToList());
            serialized.Seek(0, SeekOrigin.Begin);

            var fs = File.OpenWrite(projFilePath);
            serialized.WriteTo(fs);
            fs.Close();
        }

        public void LoadProject()
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

            fileManagerModel.ReplaceHierarchy(hierarchy);
        }
    }
}
