using System.Collections.ObjectModel;
using System.Windows.Controls;
using Editor.TrackEditor;
using Editor.GameObjectEditor;
using Editor.BillboardEditor;
using Editor.GameEntities;
using System.ComponentModel;

namespace Editor.TabbedEditors
{
    public class EditorTab : INotifyPropertyChanged
    {
        UserControl view;
        public UserControl View { get => view; }

        IEditorTabModel model;

        bool canBeClosed;
        public bool CanBeClosed { get => canBeClosed; }

        string shownName;
        public string ShownName { get => shownName; }

        public EditorTab(UserControl view, IEditorTabModel model, string shown_name, bool can_be_closed)
        {
            canBeClosed = can_be_closed;
            this.view = view;
            shownName = shown_name;
            this.model = model;

            model.PropertyChanged += (s, e) => 
            { 
                if (e.PropertyName == "IsDirty") 
                    OnPropertyChanged("IsDirty"); 
            };
        }

        public void Save()
        {
            model.ApplyChanges();
        }

        public bool IsDirty => model.IsDirty;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class TabbedEditorsModel
    {
        ObservableCollection<EditorTab> tabs = new ObservableCollection<EditorTab>();
        public ObservableCollection<EditorTab> Tabs { get => tabs; }

        public bool CloseAllTabs()
        {
            for(int i = tabs.Count - 1; i >= 0; i--)
                CloseTab(tabs[i]);

            return tabs.Count == 0;
        }

        public void CloseTab(EditorTab tab)
        {
            if(tab.IsDirty)
            {
                var modal = new ApplyChangesDialog();
                modal.ShowDialog();
                switch(modal.choise)
                {
                    case ApplyChangesDialog.Choise.Keep:
                        SaveTab(tab);
                        tabs.Remove(tab);
                        break;
                    case ApplyChangesDialog.Choise.Discard:
                        tabs.Remove(tab);
                        break;
                    case ApplyChangesDialog.Choise.ContinueEditing:
                        break;
                }
            }
            else
                tabs.Remove(tab);
        }

        public void SaveTab(EditorTab tab)
        {
            if(!MainModel.CanSaveProject)
            {
                MainModel.TryEnableSaveProject();
                if(!MainModel.CanSaveProject)
                {
                    var modal = new ProjectSaveFailed();
                    modal.ShowDialog();
                    return;
                }
            }

            tab.Save();
            MainModel.SaveProject();
        }

        public void OpenFileEditor(FileManager.File file)
        {
            switch(file.Content)
            {
                case GameObject _:
                    var go_eitor_view = new GameObjectEditorView();
                    var go_eitor_model = new GameObjectEditorModel(file);
                    (go_eitor_view.DataContext as GameObjectEditorVM).Model = go_eitor_model;
                    tabs.Add(new EditorTab(go_eitor_view, go_eitor_model, "test go name: " + file.Name, true));
                    break;
                case Billboard _:
                    var bb_editor_view = new BillboardEditorView();
                    var bb_editor_model = new BillboardEditorModel(file);
                    (bb_editor_view.DataContext as BillboardEditorVM).Model = bb_editor_model;
                    tabs.Add(new EditorTab(bb_editor_view, bb_editor_model, "test bb name: " + file.Name, true));
                    break;
                case Track _:
                    var track_editor_view = new TrackEditorView();
                    var track_editor_model = new TrackEditorModel(file);
                    (track_editor_view.DataContext as TrackEditorVM).Model = track_editor_model;
                    tabs.Add(new EditorTab(track_editor_view, track_editor_model, "test track name: " + file.Name, true));
                    break;
            }
        }
    }
}
