using Editor.GameEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Editor.FileManager
{
    public class FileManagerModel
    {
        public ObservableCollection<IContent> Hierarchy { get => root.Contents; }

        Folder root = new Folder("root", null);

        public FileManagerModel()
        {
            Bitmap bmp = new Bitmap(200, 100);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 200, 50));

            var bb = new Billboard();
            bb.AddLOD(new LOD(bmp));
            bb.Width = 4;
            bb.X = 0;
            bb.Z = -1;

            root.AddContent(new File("test_billboard_green", root, bb));

            bmp = new Bitmap(300, 100);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 100, 50, 200));

            bb = new Billboard();
            bb.AddLOD(new LOD(bmp));
            bb.Width = 4;
            bb.X = 0;
            bb.Z = -1;

            root.AddContent(new File("test_billboard_blue", root, bb));

            root.AddContent(new Folder("test", root));

            root.AddContent(new File("test go", root, new GameObject()));

            root.AddContent(new File("track", root, new Track()));
        }

        public void ReplaceHierarchy(List<IContent> hierarchy)
        {
            root.Contents.Clear();
            foreach (var content in hierarchy)
                root.AddContent(content);
        }

        public void DeleteContent(IContent content)
        {
            bool DeleteContent(IContent cont, ObservableCollection<IContent> contents)
            {
                for (int i = 0; i < contents.Count; i++)
                    if (contents[i] == cont)
                    {
                        contents.RemoveAt(i);
                        return true;
                    }

                for (int i = 0; i < contents.Count; i++)
                    if (contents[i] is Folder)
                        if (DeleteContent(cont, (contents[i] as Folder).Contents))
                            return true;

                return false;
            }

            DeleteContent(content, root.Contents);
        }

        // If new_location is folder insert inside it.
        // If new_location is file insert in it's parent.
        // If new_location is null insert in root.
        public void MoveContent(IContent content, IContent new_location)
        {
            var insert_to = new_location;
            if (insert_to is File)
                insert_to = insert_to.Parent;
            else if (insert_to == null)
                insert_to = root;

            // We can't put folder inside it's content.
            var insert_to_parent = insert_to;
            while (true)
            {
                if (insert_to_parent == null)
                    break;
                if (content == insert_to_parent)
                    return;
                insert_to_parent = insert_to_parent.Parent;
            }

            DeleteContent(content);
            content.Parent = insert_to;
            (insert_to as Folder).AddContent(content);
        }

        public void NewFolder(IContent location)
        {
            var folder = new Folder("NewFolder", null);
            MoveContent(folder, location);
        }

        public void NewBillboard(IContent location)
        {
            var file = new File("billboard", null, new Billboard());
            MoveContent(file, location);
        }

        public void NewGameObject(IContent location)
        {
            var file = new File("game object", null, new GameObject());
            MoveContent(file, location);
        }

        public void RenameItem(IContent item, string new_name)
        {
            item.Name = new_name;
            var parent = item.Parent as Folder;
            parent.Contents.Remove(item);
            parent.AddContent(item);
        }

        public void OpenFileEditor(File file)
        {
            var tabbed_editors_model = MainModel.TabbedEditorsModel;
            tabbed_editors_model.OpenFileEditor(file);
        }
    }

    public interface IContent
    {
        string Name { get; set; }
        IContent Parent { get; set; }
    }

    public class Folder : IContent, INotifyPropertyChanged
    {
        string name;
        public string Name { get => name; set { name = value; OnPropertyChanged("Name"); } }

        public IContent Parent { get; set; }

        public ObservableCollection<IContent> Contents { get => contents; }
        ObservableCollection<IContent> contents = new ObservableCollection<IContent>();

        public Folder(string name, IContent parent)
        {
            this.name = name;
            Parent = parent;
        }

        // Keep sorted.
        public void AddContent(IContent content)
        {
            for (int i = 0; i < contents.Count; i++)
                if (contents[i].Name.CompareTo(content.Name) == 1)
                {
                    contents.Insert(i, content);
                    return;
                }

            contents.Add(content);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum FileIcon
    {
        Billboard,
        GameObject,
        Track,
        Other
    }

    public class File : IContent, INotifyPropertyChanged
    {
        string name;
        public string Name { get => name; set { name = value; OnPropertyChanged("Name"); } }

        public IContent Parent { get; set; }

        ISaveableEntity content;
        public ISaveableEntity Content { get => content; set => content = value; }

        public FileIcon Icon { get => content.GetIcon(); }

        public File(string name, IContent parent, ISaveableEntity content)
        {
            this.name = name;
            Parent = parent;
            this.content = content;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}