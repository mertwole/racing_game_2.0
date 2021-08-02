using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

    class TabbedEditorsModel
    {
        ObservableCollection<EditorTab> tabs = new ObservableCollection<EditorTab>();
        public ObservableCollection<EditorTab> Tabs { get => tabs; }

        public TabbedEditorsModel()
        {
            tabs.Add(new EditorTab(new TrackEditor.TrackEditorView(), "track editor", false));

            tabs.Add(new EditorTab(new GameObjectEditor.GameObjectEditorView(), "go editor", true));
            tabs.Add(new EditorTab(new BillboardCreator.BillboardCreatorView(), "billboard editor", true));
        }
    }
}
