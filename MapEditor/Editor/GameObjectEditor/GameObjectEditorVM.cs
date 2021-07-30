using Editor.Common;
using Editor.CustomControls;
using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

namespace Editor.GameObjectEditor
{
    public class EntityDataTemplateSelector : DataTemplateSelector
    {
        DataTemplate billboardDataTemplate;
        public DataTemplate BillboardDataTemplate { get => billboardDataTemplate; set => billboardDataTemplate = value; }

        DataTemplate colliderDataTemplate;
        public DataTemplate ColliderDataTemplate { get => colliderDataTemplate; set => colliderDataTemplate = value; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Billboard)
                return billboardDataTemplate;

            if(item is Collider)
                return colliderDataTemplate;

            throw new Exception("Wrong data type : expected Billboard or Collider.");
        }
    }

    // Takes Billboard or Collider as the first value and GameObjectEtorVM as the second.
    public class ObjectOutlineVisiblityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = values[1] as GameObjectEditorVM;

            if (values[0] == vm.SelectedCollider) return Visibility.Visible;
            if (values[0] == vm.SelectedBillboard) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class GameObjectEditorVM : INotifyPropertyChanged
    {
        GameObjectEditorModel model = ModelLocator.GetModel<GameObjectEditorModel>();

        public GameObject GameObject { get => model.GameObject; }

        ObservableCollection<object> billboardsAndColliders = new ObservableCollection<object>();
        public ObservableCollection<object> BillboardsAndColliders { get => billboardsAndColliders; }

        ObservableCollection<object> xDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> XDescendingSortedCollidersAndBillboards { get => xDescendingSortedCollidersAndBillboards; }
        ObservableCollection<object> yDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> YDescendingSortedCollidersAndBillboards { get => yDescendingSortedCollidersAndBillboards; }
        ObservableCollection<object> zDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> ZDescendingSortedCollidersAndBillboards { get => zDescendingSortedCollidersAndBillboards; }

        public GameObjectEditorVM()
        {
            billboardsAndColliders.CollectionChanged += UpdateSortedCollections;

            foreach (var billboard in GameObject.Billboards)
                billboardsAndColliders.Add(billboard);
            foreach (var collider in GameObject.Colliders)
                billboardsAndColliders.Add(collider);
        }

        void InsertInSortedCollection(object item, ObservableCollection<object> collection, Comparison<object> comparison)
        {
            for(int i = 0; i < collection.Count; i++)
                if(comparison(collection[i], item) > 0)
                {
                    collection.Insert(i, item);
                    return;
                }

            collection.Add(item);
        }

        void UpdateSortedCollections(object sender, NotifyCollectionChangedEventArgs e)
        {
            int compareXDescending(object x, object y)
            {
                var x_coord = 0.0;
                if (x is Collider cx) x_coord = cx.X - cx.SizeX * 0.5;
                else if (x is Billboard bx) x_coord = bx.X - bx.Width * 0.5;
                var y_coord = 0.0;
                if (y is Collider cy) y_coord = cy.X - cy.SizeX * 0.5;
                else if (y is Billboard by) y_coord = by.X - by.Width * 0.5;

                return -Math.Sign(x_coord - y_coord);
            }

            int compareYDescending(object x, object y)
            {
                var x_coord = 0.0;
                if (x is Collider cx) x_coord = cx.Y - cx.SizeY * 0.5;
                else if (x is Billboard bx) x_coord = bx.Y - bx.Height * 0.5;
                var y_coord = 0.0;
                if (y is Collider cy) y_coord = cy.Y - cy.SizeY * 0.5;
                else if (y is Billboard by) y_coord = by.Y - by.Height * 0.5;

                return -Math.Sign(x_coord - y_coord);
            }

            int compareZDescending(object x, object y)
            {
                var x_coord = 0.0;
                if (x is Collider cx) x_coord = cx.Z - cx.SizeZ * 0.5;
                else if (x is Billboard bx) x_coord = bx.Z;
                var y_coord = 0.0;
                if (y is Collider cy) y_coord = cy.Z - cy.SizeZ * 0.5;
                else if (y is Billboard by) y_coord = by.Z;

                return -Math.Sign(x_coord - y_coord);
            }

            if(e.OldItems != null)
                foreach(var deleted in e.OldItems)
                {
                    xDescendingSortedCollidersAndBillboards.Remove(deleted);
                    yDescendingSortedCollidersAndBillboards.Remove(deleted);
                    zDescendingSortedCollidersAndBillboards.Remove(deleted);
                }

            if(e.NewItems != null)
                foreach(var added in e.NewItems)
                {
                    InsertInSortedCollection(added, xDescendingSortedCollidersAndBillboards, compareXDescending);
                    InsertInSortedCollection(added, yDescendingSortedCollidersAndBillboards, compareYDescending);
                    InsertInSortedCollection(added, zDescendingSortedCollidersAndBillboards, compareZDescending);
                }
        }

        public ICommand ApplyChanges
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyboardEventArgs;

                if ((e as KeyEventArgs).Key == Key.S && args.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                    model.ApplyChanges();
            });
        }

        T FindParentOfType<T>(DependencyObject element) where T : class
        {
            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null) return null;
            if (parent is T par) return par;
            return FindParentOfType<T>(parent);
        }

        object selectedEntity = null;

        public Collider SelectedCollider { get => selectedEntity as Collider; }
        public Billboard SelectedBillboard { get => selectedEntity as Billboard; }

        public bool ColliderSelected { get => selectedEntity is Collider; }
        public bool BillboardSelected { get => selectedEntity is Billboard; }

        // OnPropertyChanged("UpdateSelectedObject") called every time when Collider/Billboard selected/deselected.
        public bool UpdateSelectedObject { get => true; }

        public ICommand SelectObject
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;
                args.Handled = true;
                var sender = args.Source as DependencyObject;
                var inf_grid_view = FindParentOfType<InfiniteGridView>(sender);

                selectedEntity = inf_grid_view.FindItemByChildControl(sender);

                OnPropertyChanged("ColliderSelected");
                OnPropertyChanged("BillboardSelected");
                OnPropertyChanged("SelectedBillboard");
                OnPropertyChanged("SelectedCollider");
                OnPropertyChanged("UpdateSelectedObject");
            });
        }

        public ICommand UnselectObject
        {
            get => new RelayCommand((e) =>
            {
                if ((e as MouseEventArgs).LeftButton != MouseButtonState.Pressed) return;

                selectedEntity = null;

                OnPropertyChanged("ColliderSelected");
                OnPropertyChanged("BillboardSelected");
                OnPropertyChanged("SelectedBillboard");
                OnPropertyChanged("SelectedCollider");
                OnPropertyChanged("UpdateSelectedObject");
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
