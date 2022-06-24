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

        FileManager.File loadedFrom;
        ISaveableEntity editedEntity;
        IEditorTabModel model;

        bool canBeClosed = true;
        public bool CanBeClosed { get => canBeClosed; }

        string shownName;
        public string ShownName { get => shownName; }

        bool isDirty = false;
        public bool IsDirty { get => isDirty; }

        public EditorTab(FileManager.File file)
        {
            loadedFrom = file;
            editedEntity = file.Content.Clone();
            editedEntity.PropertyChanged += (s, e) => Dirtied();

            switch (editedEntity)
            {
                case GameObject go:
                    view = new GameObjectEditorView();
                    model = new GameObjectEditorModel(go);
                    (view.DataContext as GameObjectEditorVM).Model = model as GameObjectEditorModel;
                    shownName = "game object [" + "NAME TODO" + "]";
                    break;
                case Billboard bb:
                    view = new BillboardEditorView();
                    model = new BillboardEditorModel(bb);
                    (view.DataContext as BillboardEditorVM).Model = model as BillboardEditorModel;
                    shownName = "billboard [" + "NAME TODO" + "]";
                    break;
                case Track track:
                    view = new TrackEditorView();
                    model = new TrackEditorModel(track);
                    (view.DataContext as TrackEditorVM).Model = model as TrackEditorModel;
                    shownName = "track [" + "NAME TODO" + "]";
                    break;
            }
        }

        void Dirtied()
        {
            isDirty = true;
            OnPropertyChanged("IsDirty");
        }

        public void Save()
        {
            loadedFrom.Content = editedEntity.Clone();
            isDirty = false;
            OnPropertyChanged("IsDirty");
        }

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
            tabs.Add(new EditorTab(file));
        }
    }
}
