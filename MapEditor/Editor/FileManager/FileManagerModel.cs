using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Editor.FileManager
{
    public class FileManagerModel
    {
        public ObservableCollection<IContent> Hierarchy { get => hierarchy; }
        ObservableCollection<IContent> hierarchy = new ObservableCollection<IContent>();

        public FileManagerModel()
        {
            hierarchy.Add(new Folder("wddw", null));
            hierarchy.Add(new File("ttt", null));

            (hierarchy[0] as Folder).Contents.Add(new File("332", hierarchy[0]));
            (hierarchy[0] as Folder).Contents.Add(new File("wfwq", hierarchy[0]));

            var folder = new Folder("234f", hierarchy[0]);
            folder.Contents.Add(new File("dwdwdw", folder));
            folder.Parent = hierarchy[0];
            (hierarchy[0] as Folder).Contents.Add(folder);

            (hierarchy[0] as Folder).Contents.Add(new File("saf", hierarchy[0]));
        }

        public void DeleteContent(IContent content)
        {
            DeleteContent(content, hierarchy);
        }

        bool DeleteContent(IContent content, ObservableCollection<IContent> contents)
        {
            for (int i = 0; i < contents.Count; i++)
                if (contents[i] == content)
                {
                    contents.RemoveAt(i);
                    return true;
                }

            for (int i = 0; i < contents.Count; i++)
                if (contents[i] is Folder)
                    if (DeleteContent(content, (contents[i] as Folder).Contents))
                        return true;

            return false;
        }

        // If new_location is folder insert inside it.
        // If new_location is file insert in it's parent.
        public void MoveContent(IContent content, IContent new_location)
        {
            var insert_to = new_location;
            if (insert_to is File)
                insert_to = insert_to.Parent;

            // We can't put folder in itself.
            if (content == insert_to)
                return;

            DeleteContent(content);
            content.Parent = insert_to;
            if (content.Parent == null)
                hierarchy.Add(content);
            else
                (content.Parent as Folder).Contents.Add(content);
        }
    }

    public interface IContent
    {
        string Name { get; set; }
        IContent Parent { get; set; } // null means that it's the root directory.
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
