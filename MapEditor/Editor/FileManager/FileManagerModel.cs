using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.FileManager
{
    public class FileManagerModel
    {
        public ObservableCollection<IContent> Hierarchy { get => root.Contents; }

        Folder root = new Folder("root", null);

        public FileManagerModel()
        {
            root.AddContent(new Folder("1", root));
            root.AddContent(new File("2", root));

            (root.Contents[0] as Folder).AddContent(new File("3", root.Contents[0]));
            (root.Contents[0] as Folder).AddContent(new File("4", root.Contents[0]));

            var folder = new Folder("5", root.Contents[0]);
            folder.AddContent(new File("6", folder));
            folder.Parent = root.Contents[0];
            (root.Contents[0] as Folder).AddContent(folder);

            (root.Contents[0] as Folder).AddContent(new File("7", root.Contents[0]));
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
            while(true)
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

    public class File : IContent, INotifyPropertyChanged
    {
        string name;
        public string Name { get => name; set { name = value; OnPropertyChanged("Name"); } }

        public IContent Parent { get; set; }

        public File(string name, IContent parent)
        {
            this.name = name;
            Parent = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
