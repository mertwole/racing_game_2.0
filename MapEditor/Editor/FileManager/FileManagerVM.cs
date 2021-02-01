using Editor.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.FileManager
{
    public class FileManagerVM
    {
        FileManagerModel model = new FileManagerModel();

        public ObservableCollection<IContent> Hierarchy { get => model.Hierarchy; }

        // MainTreeView
        public static readonly DependencyProperty MainTreeViewProperty =
        DependencyProperty.RegisterAttached(
        "MainTreeView", typeof(TreeView),
        typeof(FileManagerVM), new FrameworkPropertyMetadata(OnMainTreeViewChanged));

        public static void SetMainTreeView(DependencyObject element, TreeView value) => element.SetValue(MainTreeViewProperty, value);
        public static TreeView GetMainTreeView(DependencyObject element) => (TreeView)element.GetValue(MainTreeViewProperty);

        static TreeView mainTreeView = null;
        public static void OnMainTreeViewChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args) => mainTreeView = obj as TreeView;

        #region Renaming

        IContent editingItem = null;
        string editingItemPrevName = "";

        public ICommand StartRenameItem
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;

                if (args.ClickCount == 2)
                {
                    args.Handled = true;
                    (args.Source as TextBlock).Focus();
                    editingItem = ((args.Source as TextBlock).DataContext as IContent);
                    editingItemPrevName = editingItem.Name;
                    editingItem.Name = "";
                }
            });
        }

        public ICommand ItemNameInput
        {
            get => new RelayCommand((e) =>
            {
                var args = e as TextCompositionEventArgs;

                if (editingItem == null)
                    return;

                if (Keyboard.IsKeyDown(Key.Return))
                {
                    FinishRenameItem.Execute(null);
                }
                else if(Keyboard.IsKeyDown(Key.Back))
                {
                    var len = editingItem.Name.Length;
                    if(len != 0)
                        editingItem.Name = editingItem.Name.Remove(len - 1);
                }
                else
                {
                    var text = args.Text;
                    if (text.Length != 1)
                        return;

                    bool cap = Keyboard.IsKeyDown(Key.LeftShift) ^ Keyboard.IsKeyDown(Key.CapsLock);
                    text = cap ? text.ToUpper() : text.ToLower();
                    editingItem.Name += text;
                }
            });
        }

        public ICommand FinishRenameItem
        {
            get => new RelayCommand((e) =>
            {
                if (editingItem == null)
                    return;

                if (editingItem.Name == "")
                    editingItem.Name = editingItemPrevName;

                editingItem = null;
            });
        }

        #endregion

        #region DragDrop



        #endregion
    }
}
