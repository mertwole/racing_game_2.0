﻿using Editor.Common;
using Editor.CustomControls;
using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;
using Editor.FileManager;
using System.Drawing.Drawing2D;

namespace Editor.GameObjectEditor
{
    public class AverageImageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1, 1);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(value as System.Drawing.Bitmap, new System.Drawing.Rectangle(0, 0, 1, 1));
            }
            var px = bmp.GetPixel(0, 0);
            px = System.Drawing.Color.FromArgb(255, px.R, px.G, px.B);
            bmp.SetPixel(0, 0, px);

            return BitmapToImageSourceConverter.Convert(bmp);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class EntityDataTemplateSelector : DataTemplateSelector
    {
        DataTemplate billboardDataTemplate;
        public DataTemplate BillboardDataTemplate { get => billboardDataTemplate; set => billboardDataTemplate = value; }

        DataTemplate colliderDataTemplate;
        public DataTemplate ColliderDataTemplate { get => colliderDataTemplate; set => colliderDataTemplate = value; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PositionedBillboard)
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
        GameObjectEditorModel model;
        public GameObjectEditorModel Model { set { model = value; Init(); OnPropertyChanged("GameObject"); } }
        public GameObject GameObject { get => model.GameObject; }

        ObservableCollection<object> billboardsAndColliders = new ObservableCollection<object>();
        public ObservableCollection<object> BillboardsAndColliders { get => billboardsAndColliders; }

        ObservableCollection<object> xDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> XDescendingSortedCollidersAndBillboards { get => xDescendingSortedCollidersAndBillboards; }
        ObservableCollection<object> yDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> YDescendingSortedCollidersAndBillboards { get => yDescendingSortedCollidersAndBillboards; }
        ObservableCollection<object> zDescendingSortedCollidersAndBillboards = new ObservableCollection<object>();
        public ObservableCollection<object> ZDescendingSortedCollidersAndBillboards { get => zDescendingSortedCollidersAndBillboards; }

        void Init()
        {
            GameObject.Billboards.ListChanged += UpdateBillboardsAndCollidersCollection;
            GameObject.Colliders.ListChanged += UpdateBillboardsAndCollidersCollection;

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

        void UpdateBillboardsAndCollidersCollection(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                object added;
                if (sender is BindingList<PositionedBillboard> sender_bb)
                    added = sender_bb[e.NewIndex];
                else
                    added = (sender as BindingList<Collider>)[e.NewIndex];

                billboardsAndColliders.Add(added);
                xDescendingSortedCollidersAndBillboards.Add(added);
                yDescendingSortedCollidersAndBillboards.Add(added);
                zDescendingSortedCollidersAndBillboards.Add(added);

                UpdateSortedCollections();
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted) 
            {
                object removed;
                if (sender is BindingList<PositionedBillboard> sender_bb)
                    removed = sender_bb[e.OldIndex];
                else
                    removed = (sender as BindingList<Collider>)[e.OldIndex];

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
            else if(obj is PositionedBillboard billboard)
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

        T FindParentOfType<T>(DependencyObject element) where T : class
        {
            if (element is T) return element as T;

            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null) return null;
            if (parent is T par) return par;
            return FindParentOfType<T>(parent);
        }

        object selectedEntity = null;

        public Collider SelectedCollider { get => selectedEntity as Collider; }
        public PositionedBillboard SelectedBillboard { get => selectedEntity as PositionedBillboard; }

        public bool ColliderSelected { get => selectedEntity is Collider; }
        public bool BillboardSelected { get => selectedEntity is PositionedBillboard; }

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
                movedObjectGridView = FindParentOfType<InfiniteGridView>(sender);
                var obj = movedObjectGridView.FindItemByChildControl(sender);

                startMoveScreenPos = args.GetPosition(movedObjectGridView);
                if (obj is Collider collider) startMoveWorldPos = collider.Position;
                if (obj is PositionedBillboard billboard) startMoveWorldPos = billboard.Position;

                model.StartMoveObject(obj);
                Mouse.Capture(sender as IInputElement);

                movedObject = sender;
                movingObject = true;
            });
        }

        DependencyObject movedObject;
        InfiniteGridView movedObjectGridView;

        public ICommand MoveObject
        {
            get => new RelayCommand((e) =>
            {
                if (!movingObject) return;

                var args = e as MouseEventArgs;
                if (args.LeftButton != MouseButtonState.Pressed) return;

                var obj = movedObjectGridView.FindItemByChildControl(movedObject);
                var new_pos_screen_space = args.GetPosition(movedObjectGridView);
                var delta_move = new_pos_screen_space - startMoveScreenPos;
                delta_move.Y *= -1;
                var delta_world = movedObjectGridView.ScreenSpaceToWorldSize(new Point(delta_move.X, delta_move.Y));

                Vector3 new_pos = new Vector3();
                if (obj is PositionedBillboard bb) new_pos = bb.Position;
                if (obj is Collider collider) new_pos = collider.Position;

                switch (movedObjectGridView.Name)
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
