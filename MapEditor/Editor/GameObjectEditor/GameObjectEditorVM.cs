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
using System.Collections.Generic;
using Editor.FileManager;

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
            GameObject.Billboards.CollectionChanged += UpdateBillboardsAndCollidersCollection;
            GameObject.Colliders.CollectionChanged += UpdateBillboardsAndCollidersCollection;

            InitialBillboardsAndCollidersFill(GameObject);
        }

        #region BillboardsAndColliders stuff

        void InitialBillboardsAndCollidersFill(GameObject gameObject)
        {
            foreach (var billboard in GameObject.Billboards)
            {
                billboardsAndColliders.Add(billboard);

                xDescendingSortedCollidersAndBillboards.Add(billboard);
                yDescendingSortedCollidersAndBillboards.Add(billboard);
                zDescendingSortedCollidersAndBillboards.Add(billboard);
            }
            foreach (var collider in GameObject.Colliders)
            {
                billboardsAndColliders.Add(collider);

                xDescendingSortedCollidersAndBillboards.Add(collider);
                yDescendingSortedCollidersAndBillboards.Add(collider);
                zDescendingSortedCollidersAndBillboards.Add(collider);
            }

            UpdateSortedCollections();
        }

        void UpdateBillboardsAndCollidersCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var added in e.NewItems)
                {
                    billboardsAndColliders.Add(added);

                    xDescendingSortedCollidersAndBillboards.Add(added);
                    yDescendingSortedCollidersAndBillboards.Add(added);
                    zDescendingSortedCollidersAndBillboards.Add(added);

                    UpdateSortedCollections();
                }
                    

            if (e.OldItems != null)
                foreach (var removed in e.OldItems)
                {
                    billboardsAndColliders.Remove(removed);

                    xDescendingSortedCollidersAndBillboards.Remove(removed);
                    yDescendingSortedCollidersAndBillboards.Remove(removed);
                    zDescendingSortedCollidersAndBillboards.Remove(removed);
                }
        }

        Vector3 GetClosestToViewerPoint(object obj)
        {
            if(obj is Collider collider)
            {
                return new Vector3(
                    collider.X - collider.SizeX * 0.5, 
                    collider.Y - collider.SizeY * 0.5, 
                    collider.Z - collider.SizeZ * 0.5);
            }
            else if(obj is Billboard billboard)
            {
                return new Vector3(
                    billboard.X - billboard.Width * 0.5,
                    billboard.Y - billboard.Height * 0.5,
                    billboard.Z);
            }

            return new Vector3();
        }

        int compareXDescending(object x, object y) =>
            -Math.Sign(GetClosestToViewerPoint(x).X - GetClosestToViewerPoint(y).X);

        int compareYDescending(object x, object y) =>
            -Math.Sign(GetClosestToViewerPoint(x).Y - GetClosestToViewerPoint(y).Y);

        int compareZDescending(object x, object y) =>
            -Math.Sign(GetClosestToViewerPoint(x).Z - GetClosestToViewerPoint(y).Z);

        void OrderObservableCollectionByList(ObservableCollection<object> observable, List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (observable.IndexOf(list[i]) != i)
                {
                    observable.Remove(list[i]);
                    observable.Insert(i, list[i]);
                }
            }
        }

        void UpdateSortedCollections()
        {
            var items = new List<object>(billboardsAndColliders);

            items.Sort(compareXDescending);
            OrderObservableCollectionByList(xDescendingSortedCollidersAndBillboards, items);

            items.Sort(compareYDescending);
            OrderObservableCollectionByList(yDescendingSortedCollidersAndBillboards, items);

            items.Sort(compareZDescending);
            OrderObservableCollectionByList(zDescendingSortedCollidersAndBillboards, items);
        }

        #endregion

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
                Keyboard.Focus(sender as IInputElement); // Focus for delete operation.

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

        bool movingObject = false;
        Point startMoveScreenPos;
        Vector3 startMoveWorldPos;

        public ICommand StartMoveObject
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;

                if (args.LeftButton == MouseButtonState.Released) return;

                var sender = args.Source as DependencyObject;
                var inf_grid_view = FindParentOfType<InfiniteGridView>(sender);
                var obj = inf_grid_view.FindItemByChildControl(sender);

                startMoveScreenPos = args.GetPosition(inf_grid_view);
                if (obj is Collider collider) startMoveWorldPos = collider.Position;
                if (obj is Billboard billboard) startMoveWorldPos = billboard.Position;

                model.StartMoveObject(obj);
                Mouse.Capture(sender as IInputElement);

                movingObject = true;
            });
        }

        public ICommand MoveObject
        {
            get => new RelayCommand((e) =>
            {
                if (!movingObject) return;

                var args = e as MouseEventArgs;

                if (args.LeftButton != MouseButtonState.Pressed) return;

                var sender = args.Source as DependencyObject;

                var inf_grid_view = FindParentOfType<InfiniteGridView>(sender);
                var obj = inf_grid_view.FindItemByChildControl(sender);

                var new_pos_screen_space = args.GetPosition(inf_grid_view);
                var delta_move = new_pos_screen_space - startMoveScreenPos;
                delta_move.Y *= -1;
                var delta_world = inf_grid_view.ScreenSpaceToWorldSize(new Point(delta_move.X, delta_move.Y));

                Vector3 new_pos = new Vector3();
                if (obj is Billboard bb) new_pos = bb.Position;
                if (obj is Collider collider) new_pos = collider.Position;

                switch (inf_grid_view.Name)
                {
                    case "TopView":
                        new_pos.X = startMoveWorldPos.X + delta_world.X;
                        new_pos.Z = startMoveWorldPos.Z + delta_world.Y;
                        break;
                    case "RightView":
                        new_pos.Z = startMoveWorldPos.Z + delta_world.X;
                        new_pos.Y = startMoveWorldPos.Y + delta_world.Y;
                        break;
                    case "BackView":
                        new_pos.X = startMoveWorldPos.X + delta_world.X;
                        new_pos.Y = startMoveWorldPos.Y + delta_world.Y;
                        break;
                }

                model.MoveObject(new_pos);

                UpdateSortedCollections();
            });
        }

        public ICommand FinishMoveObject
        {
            get => new RelayCommand((e) =>
            {
                if ((e as MouseEventArgs).LeftButton == MouseButtonState.Pressed) return;

                movingObject = false;
                Mouse.Capture(null);
            });
        }

        public ICommand DeleteObject
        {
            get => new RelayCommand((e) =>
            {
                if ((e as KeyEventArgs).Key != Key.Delete) return;

                model.DeleteObject(selectedEntity);

                selectedEntity = null;

                OnPropertyChanged("ColliderSelected");
                OnPropertyChanged("BillboardSelected");
                OnPropertyChanged("SelectedBillboard");
                OnPropertyChanged("SelectedCollider");
                OnPropertyChanged("UpdateSelectedObject");
            });
        }

        public ICommand AddCollider
        {
            get => new RelayCommand((e) =>
            {
                if ((e as MouseEventArgs).LeftButton == MouseButtonState.Released) return;

                model.AddCollider();
            });
        }

        public ICommand DropBillboard
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;

                var contents = (HashSet<IContent>)args.Data.GetData(typeof(HashSet<IContent>));
                if (contents == null) return;

                var files = new List<File>();
                foreach (var content in contents)
                    if (content is File file)
                        files.Add(file);

                foreach (var file in files)
                    model.AddBillboardFromFile(file);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
