using Editor.Common;
using Editor.GameEntities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor.BillboardEditor
{
    public class CheckSelectionVisiblity : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            (int)values[0] == (int)values[1] ? Visibility.Visible : Visibility.Hidden;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
    }

    public class CheckSelectionVisiblityInverted : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            (int)values[0] == (int)values[1] ? Visibility.Hidden : Visibility.Visible;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
    }

    public class BillboardEditorVM : INotifyPropertyChanged
    {
        BillboardEditorModel model;
        public BillboardEditorModel Model { set { model = value; OnPropertyChanged("LODs"); } }
        public ObservableCollection<LOD> LODs { get => model == null ? new ObservableCollection<LOD>() : model.LODs; }

        public Visibility DropRegionsVisiblity { get => draggedImage == null ? Visibility.Hidden : Visibility.Visible; }

        UIElement draggedImage;

        int selectionId = -1;
        public int SelectionId { 
            get => selectionId; 
            private set{ 
                selectionId = value; 
                OnPropertyChanged("SelectionId"); 
            } 
        }

        public ICommand StartDragImage
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;
                
                if(args.LeftButton == MouseButtonState.Pressed)
                {
                    draggedImage = args.Source as UIElement;
                    OnPropertyChanged("DropRegionsVisiblity");

                    var dd = DragDrop.DoDragDrop(draggedImage, draggedImage, DragDropEffects.Move);
                    if(dd == DragDropEffects.None)
                    {
                        draggedImage = null;
                        OnPropertyChanged("DropRegionsVisiblity");
                    }
                }
            });
        }

        DependencyObject GetParentContentPresenter(object element)
        {
            if (element is ContentPresenter)
                return element as DependencyObject;

            var parent = VisualTreeHelper.GetParent(element as DependencyObject);
            if (parent == null) return null;

            return GetParentContentPresenter(parent);
        }

        public ICommand DropImage
        {
            get => new RelayCommand((e) =>
            {
                var args = e as DragEventArgs;

                if (draggedImage == null) return;

                var drag_src = GetParentContentPresenter(draggedImage);
                var src_id = ItemsControl.GetAlternationIndex(drag_src);

                var drag_dest = GetParentContentPresenter(args.Source);
                var dest_id = ItemsControl.GetAlternationIndex(drag_dest);

                if (dest_id > src_id) dest_id--;

                model.MoveLODTo(src_id, dest_id);

                SelectionId = dest_id;

                draggedImage = null;
                OnPropertyChanged("DropRegionsVisiblity");
            });
        }

        public ICommand DropImageToEnd
        {
            get => new RelayCommand((e) =>
            {
                var drag_src = GetParentContentPresenter(draggedImage);
                var src_id = ItemsControl.GetAlternationIndex(drag_src);

                model.MoveLODTo(src_id, LODs.Count - 1);

                SelectionId = LODs.Count - 1;

                draggedImage = null;
                OnPropertyChanged("DropRegionsVisiblity");
            });
        }

        public ICommand AddLOD
        {
            get => new RelayCommand((e) =>
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.BMP; *.PNG) | *.BMP;*.PNG";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == true)
                    foreach(var file_name in openFileDialog.FileNames)
                        model.AddLOD(file_name);
            });
        }

        public ICommand SelectLOD
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;

                var selection = GetParentContentPresenter(args.Source);
                SelectionId = ItemsControl.GetAlternationIndex(selection);
            });
        }

        public ICommand DeleteSelected
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyboardEventArgs;

                if (args.KeyboardDevice.IsKeyDown(Key.Delete))
                    model.DeleteLOD(selectionId);                            
            });
        }

        public ICommand ApplyChanges
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyboardEventArgs;

                if((e as KeyEventArgs).Key == Key.S && args.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                    model.ApplyChanges();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
