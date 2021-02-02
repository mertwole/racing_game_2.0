using Editor.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace Editor.FileManager
{
    public static class IContentExtensions
    {
        public static Visibility CheckSelected(this IContent content)
        {
            bool selected = FileManagerVM.SelectedItems.Contains(content);
            return selected ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public static class ItemContainerGeneratorExtensions
    {
        public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search != null)
                    return search;
            }
            return null;
        }
    }

    public class FileManagerVM : INotifyPropertyChanged
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

                // If selectedItems.Count > 1 then it's mis-doubleclick.
                if (args.ClickCount == 2 && selectedItems.Count <= 1)
                {
                    args.Handled = true;
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

        #region Multiselection

        HashSet<IContent> selectedItems = new HashSet<IContent>();
        public static HashSet<IContent> SelectedItems { get => instance.selectedItems; }
        IContent lastSelected;

        void UpdateSelection(IContent content)
        {
            mainTreeView.UpdateLayout();

            var tree_view_item = mainTreeView.ItemContainerGenerator.ContainerFromItemRecursive(content);
            if (tree_view_item == null)
                return;
            // Disable default selection behaviour.
            tree_view_item.IsSelected = false;
            var selection = ChildFinder.FindChild<FrameworkElement>(tree_view_item, "Selection");
            var binding_expression = (selection).GetBindingExpression(StackPanel.VisibilityProperty);
            binding_expression.UpdateTarget();
        }

        void UnselectAll()
        {
            var old_selected = selectedItems.ToList();
            selectedItems.Clear();
            foreach (var sel in old_selected)
                UpdateSelection(sel);
        }

        public ICommand SelectItem
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedPropertyChangedEventArgs<object>;

                if (args.NewValue == null)
                    return;

                var selected = args.NewValue as IContent;

                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if(!selectedItems.Contains(selected))
                        UnselectAll();
                    // Elsewhere : maybe dragging multiple contents. Check it in SelectItemEnded.

                    lastSelected = selected;
                }

                if (!selectedItems.Contains(selected))
                    selectedItems.Add(selected);
                UpdateSelection(selected);
            });
        }

        public ICommand SelectItemEnded
        {
            get => new RelayCommand((e) =>
            {
                if (!itemsDrag && !Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    UnselectAll();
                    selectedItems.Add(lastSelected);
                    UpdateSelection(lastSelected);
                }
            });
        }

        public ICommand UnselectAllItems
        {
            get => new RelayCommand((e) =>
            {
                var new_focus = Keyboard.FocusedElement;
                if (!(new_focus is TreeViewItem))
                    UnselectAll();
            });
        }

        #endregion

        #region Delete

        public ICommand DeleteItems
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyEventArgs;

                if (args.Key == Key.Delete)
                {
                    var to_delete = selectedItems.ToList();
                    selectedItems.Clear();
                    for (int i = 0; i < to_delete.Count; i++)
                        model.DeleteContent(to_delete[i]);

                    UnselectAll();
                }
            });
        }

        #endregion

        #region DragDrop

        bool itemsDrag = false;
        // Position is relative to mainTreeView.
        IContent GetContentAtPosition(Point position)
        {
            var curr_item = mainTreeView.InputHitTest(position) as DependencyObject;
            while (true)
            {
                if (curr_item is TreeViewItem tvi)
                    return tvi.Header as IContent;
                else if (curr_item == null)
                    return null;

                curr_item = VisualTreeHelper.GetParent(curr_item);
            }
        }

        public ICommand DropItems
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                args.Handled = true;

                var mouse_pos = args.GetPosition(mainTreeView);
                var new_location = GetContentAtPosition(mouse_pos);

                var selected_items = new List<IContent>(selectedItems.AsEnumerable());
                for (int i = 0; i < selectedItems.Count; i++)
                    model.MoveContent(selected_items[i], new_location);
                UnselectAll();
            });
        }

        public ICommand StartDragItems
        {
            get => new RelayCommand((e) => itemsDrag = true);
        }

        public ICommand StopDragItems
        {
            get => new RelayCommand((e) => itemsDrag = false);
        }

        public ICommand DragItems
        {
            get => new RelayCommand((e) =>
            {
                if (!itemsDrag) return;
                itemsDrag = false;
                DragDrop.DoDragDrop(mainTreeView, selectedItems, DragDropEffects.Copy);
            });
        }

        #endregion

        static FileManagerVM instance;
        public FileManagerVM()
        {
            if (instance != null)
                throw new System.Exception("Trying to create another instance of singleton FileManagerVM");
            instance = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
