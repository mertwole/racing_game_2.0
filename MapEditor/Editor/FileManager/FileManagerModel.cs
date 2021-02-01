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
            (hierarchy[0] as Folder).Contents.Add(new Folder("234f", hierarchy[0]));
            (hierarchy[0] as Folder).Contents.Add(new File("saf", hierarchy[0]));
        }
    }

    public interface IContent
    {
        string Name { get; set; }
        IContent Parent { get; } // null means that it's the root directory.
    }

    public class Folder : IContent, INotifyPropertyChanged
    {
        string name;
        public string Name { get => name; set { name = value; OnPropertyChanged("Name"); } }

        public IContent Parent { get; private set; }

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

        public IContent Parent { get; private set; }

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
