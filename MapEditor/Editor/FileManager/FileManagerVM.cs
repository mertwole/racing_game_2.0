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
        // Returns opacity.
        public static double CheckSelected(this IContent content)
        {
            bool selected = FileManagerVM.SelectedItems.Contains(content);
            return selected ? 1.0 : 0.0;
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
                else if (Keyboard.IsKeyDown(Key.Back))
                {
                    var len = editingItem.Name.Length;
                    if (len != 0)
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

        void UnselectAll()
        {
            var old_selected = selectedItems.ToList();
            selectedItems.Clear();
            foreach (var sel in old_selected)
                UpdateSelection(sel);
        }

        void UpdateSelection(IContent content)
        {
            mainTreeView.UpdateLayout();

            var tree_view_item = mainTreeView.ItemContainerGenerator.ContainerFromItemRecursive(content);
            if (tree_view_item == null)
                return;
            // Disable default selection behaviour.
            tree_view_item.IsSelected = false;
            var selection = ChildFinder.FindChild<FrameworkElement>(tree_view_item, "Selection");
            var binding_expression = (selection).GetBindingExpression(StackPanel.OpacityProperty);
            binding_expression.UpdateTarget();
        }

        #region Multiselection

        HashSet<IContent> selectedItems = new HashSet<IContent>();
        public static HashSet<IContent> SelectedItems { get => instance.selectedItems; }
        IContent lastSelected;

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
                    if (!selectedItems.Contains(selected))
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

        #region DragDrop

        bool itemsDrag = false;

        FrameworkElement FindChildByName(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            int child_count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < child_count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child != null && child.Name == name)
                    return child;
                else
                {
                    var childs_child = FindChildByName(child, name);
                    if (childs_child != null)
                        return childs_child;
                }
            }

            return null;
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

                if (lastDragHighlight != null)
                    lastDragHighlight.Opacity = 0;
            });
        }

        public ICommand StartDragItems
        {
            get => new RelayCommand((e) => {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    itemsDrag = true;
            });
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

        FrameworkElement lastDragHighlight = null;

        public ICommand DragItemsOver
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                FrameworkElement highlight_tvi = args.OriginalSource as FrameworkElement;
                while (true)
                {
                    if (highlight_tvi is TreeViewItem)
                        break;
                    else
                        highlight_tvi = VisualTreeHelper
                        .GetParent(highlight_tvi) as FrameworkElement;
                }
                var highlight = FindChildByName(highlight_tvi, "DragHighlight");
                lastDragHighlight = highlight;
                if (highlight != null) highlight.Opacity = 1;
            });
        }

        public ICommand DragItemsLeave
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                FrameworkElement highlight_tvi = args.OriginalSource as FrameworkElement;
                while (true)
                {
                    if (highlight_tvi is TreeViewItem)
                        break;
                    else
                        highlight_tvi = VisualTreeHelper
                        .GetParent(highlight_tvi) as FrameworkElement;
                }
                var highlight = FindChildByName(highlight_tvi, "DragHighlight");
                if (highlight != null) highlight.Opacity = 0;
            });
        }

        #endregion

        #region Context menu

        public ICommand NewFolderContextMenu
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;

                var menu_item = args.Source as MenuItem;
                var context_menu = menu_item.Parent as ContextMenu;
                context_menu.Visibility = Visibility.Hidden;

                var context_menu_tvi = context_menu.PlacementTarget as FrameworkElement;
                while (true)
                {
                    if (context_menu_tvi is TreeViewItem)
                        break;
                    else
                        context_menu_tvi = VisualTreeHelper
                        .GetParent(context_menu_tvi) as FrameworkElement;
                }

                var location = (context_menu_tvi as TreeViewItem).Header;
                model.NewFolder(location as IContent);
            });
        }

        public ICommand ContextMenuOpened
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;

                var context_menu = args.Source as ContextMenu;
                var context_menu_tvi = context_menu.PlacementTarget as FrameworkElement;
                while (true)
                {
                    if (context_menu_tvi is TreeViewItem)
                        break;
                    else
                        context_menu_tvi = VisualTreeHelper
                        .GetParent(context_menu_tvi) as FrameworkElement;
                }

                UnselectAll();
                var content = (context_menu_tvi as TreeViewItem).Header as IContent;
                selectedItems.Add(content);
                UpdateSelection(content);
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