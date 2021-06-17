using Editor.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Globalization;
using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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

    public class CallCheckSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IContent).CheckSelected();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileIconEnumToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var file_icon = (FileIcon)value;

            switch(file_icon)
            {
                case FileIcon.Billboard: { return new BitmapImage(new Uri("pack://application:,,,/Editor;component/Images/billboard_icon.png")); }
                case FileIcon.GameObject: { return new BitmapImage(new Uri("pack://application:,,,/Editor;component/Images/gameobject_icon.png")); }
                case FileIcon.Other: { return new BitmapImage(new Uri("pack://application:,,,/Editor;component/Images/other_icon.png")); }

                default: { throw new Exception("Unexpected FileIcon variant"); }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class FileManagerVM : INotifyPropertyChanged
    {
        FileManagerModel model = ModelLocator.GetModel<FileManagerModel>();

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
        TextBox nameTextBox;

        public ICommand StartRenameItem
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseButtonEventArgs;

                // If selectedItems.Count > 1 then it's mis-doubleclick.
                if (args.ClickCount == 2 && selectedItems.Count <= 1)
                {
                    args.Handled = true;

                    var text_block = args.Source as TextBlock;
                    var parent = VisualTreeHelper.GetParent(text_block) as FrameworkElement;
                    var text_box = ChildFinder.FindChild<TextBox>(parent, "NameTextBox");

                    text_box.Visibility = Visibility.Visible;
                    text_box.Focus();
                    text_box.CaretIndex = 10000;
                    text_box.Select(0, 10000);

                    nameTextBox = text_box;
                    editingItem = ((args.Source as TextBlock).DataContext as IContent);
                    editingItemPrevName = editingItem.Name;

                    return;
                }
            });
        }

        public ICommand FinishRenameItemByEnter
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyEventArgs;
                if (args.Key != Key.Enter)
                    return;

                FinishRenameItem.Execute(null);
            });
        }

        public ICommand FinishRenameItem
        {
            get => new RelayCommand((e) =>
            {
                if (editingItem == null)
                    return;

                var new_name = nameTextBox.Text;
                if (new_name == "")
                    new_name = editingItemPrevName;

                model.RenameItem(editingItem, new_name);

                nameTextBox.Visibility = Visibility.Hidden;

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
            if (selection == null)
                return;
            var binding_expression = selection.GetBindingExpression(StackPanel.OpacityProperty);
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

        public ICommand DropItems
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                args.Handled = true;

                var mouse_pos = args.GetPosition(mainTreeView);
                var new_location = GetContentAtPosition(mouse_pos);

                if (lastDragHighlight != null)
                    lastDragHighlight.Opacity = 0;

                // Drag item into itself.
                if (selectedItems.Count == 1 && selectedItems.Contains(new_location))
                    return;

                var selected_items = new List<IContent>(selectedItems.AsEnumerable());
                for (int i = 0; i < selectedItems.Count; i++)
                    model.MoveContent(selected_items[i], new_location);
                UnselectAll();
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

        public ICommand DragItemsEnter
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;
                FrameworkElement highlight_tvi = args.OriginalSource as FrameworkElement;
                while (true)
                {
                    if (highlight_tvi is TreeViewItem)
                        break;
                    else if (highlight_tvi == null)
                        return;
                    else
                        highlight_tvi = VisualTreeHelper
                        .GetParent(highlight_tvi) as FrameworkElement;
                }
                var highlight = ChildFinder.FindChild<FrameworkElement>(highlight_tvi, "DragHighlight");
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
                    else if (highlight_tvi == null)
                        return;
                    else
                        highlight_tvi = VisualTreeHelper
                        .GetParent(highlight_tvi) as FrameworkElement;
                }
                var highlight = ChildFinder.FindChild<FrameworkElement>(highlight_tvi, "DragHighlight");
                if (highlight != null) highlight.Opacity = 0;
            });
        }

        #endregion

        #region Context menu

        TreeViewItem GetTreeViewItemByContextMenuItem(MenuItem item)
        {
            var context_menu = item.Parent as ContextMenu;
            var context_menu_tvi = context_menu.PlacementTarget as FrameworkElement;
            while (true)
            {
                if (context_menu_tvi is TreeViewItem)
                    break;
                else
                    context_menu_tvi = VisualTreeHelper
                    .GetParent(context_menu_tvi) as FrameworkElement;
            }

            return context_menu_tvi as TreeViewItem;
        }

        public ICommand NewFolderContextMenu
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var context_menu_tvi = GetTreeViewItemByContextMenuItem(args.Source as MenuItem);

                var location = context_menu_tvi.Header;
                model.NewFolder(location as IContent);
            });
        }

        public ICommand NewBillboardContextMenu
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var context_menu_tvi = GetTreeViewItemByContextMenuItem(args.Source as MenuItem);

                var location = context_menu_tvi.Header;
                model.NewBillboard(location as IContent);
            });
        }

        public ICommand NewGameObjectContextMenu
        {
            get => new RelayCommand((e) =>
            {
                var args = e as RoutedEventArgs;
                var context_menu_tvi = GetTreeViewItemByContextMenuItem(args.Source as MenuItem);

                var location = context_menu_tvi.Header;
                model.NewGameObject(location as IContent);
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