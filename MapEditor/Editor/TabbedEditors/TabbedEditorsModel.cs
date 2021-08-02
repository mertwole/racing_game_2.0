using System.Collections.ObjectModel;
using System.Windows.Controls;
using Editor.TrackEditor;
using Editor.GameObjectEditor;
using Editor.BillboardEditor;
using System.Drawing;
using Editor.GameEntities;

namespace Editor.TabbedEditors
{
    public class EditorTab
    {
        UserControl view;
        public UserControl View { get => view; }

        bool canBeClosed;
        public bool CanBeClosed { get => canBeClosed; }

        string shownName;
        public string ShownName { get => shownName; }

        public EditorTab(UserControl view, string shown_name, bool can_be_closed)
        {
            canBeClosed = can_be_closed;
            this.view = view;
            shownName = shown_name;
        }
    }

    public class TabbedEditorsModel
    {
        ObservableCollection<EditorTab> tabs = new ObservableCollection<EditorTab>();
        public ObservableCollection<EditorTab> Tabs { get => tabs; }

        public TabbedEditorsModel()
        {
            tabs.Add(new EditorTab(new TrackEditorView(), "track editor", false));

            var go_editor_view = new GameObjectEditorView();
            (go_editor_view.DataContext as GameObjectEditorVM).Model = new GameObjectEditorModel(new GameObject());
            tabs.Add(new EditorTab(go_editor_view, "go editor", true));

            var billboard_Creator_view = new BillboardEditorView();

            Bitmap bmp = new Bitmap(200, 100);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 200, 50));

            var bb = new Billboard();
            bb.AddLOD(new LOD(bmp));
            bb.Width = 4;
            bb.X = 0;
            bb.Z = -1;

            (billboard_Creator_view.DataContext as BillboardEditorVM).Model = new BillboardEditorModel(bb);
            tabs.Add(new EditorTab(billboard_Creator_view, "billboard editor", true));
        }

        public void CloseTab(EditorTab tab)
        {
            tabs.Remove(tab);
        }

        public void SaveTab(EditorTab tab)
        {

        }

        public void OpenFileEditor(FileManager.File file)
        {

        }
    }
}
