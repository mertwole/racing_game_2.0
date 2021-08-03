﻿using System.Collections.ObjectModel;
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

        public TabbedEditorsModel()
        {
            tabs.Add(new EditorTab(new TrackEditorView(), new TrackEditorModel(), "track editor", false));

            var file = new FileManager.File("test go", null, new GameObject());
            OpenFileEditor(file);
        }

        public void CloseTab(EditorTab tab)
        {
            if(tab.IsDirty)
            {
                // TODO : Ask to save.
            }
            else
                tabs.Remove(tab);
        }

        public void SaveTab(EditorTab tab)
        {
            tab.Save();
        }

        public void OpenFileEditor(FileManager.File file)
        {
            switch(file.Content)
            {
                case GameObject _:
                    var go_eitor_view = new GameObjectEditorView();
                    var go_eitor_model = new GameObjectEditorModel(new GameObject());
                    (go_eitor_view.DataContext as GameObjectEditorVM).Model = go_eitor_model;
                    go_eitor_model.LoadFromFile(file);
                    tabs.Add(new EditorTab(go_eitor_view, go_eitor_model, "test go name: " + file.Name, true));
                    break;
                case Billboard _:
                    var bb_editor_view = new BillboardEditorView();
                    var bb_editor_model = new BillboardEditorModel(new Billboard());
                    (bb_editor_view.DataContext as BillboardEditorVM).Model = bb_editor_model;
                    tabs.Add(new EditorTab(bb_editor_view, bb_editor_model, "test bb name: " + file.Name, true));
                    break;
            }
        }
    }
}
