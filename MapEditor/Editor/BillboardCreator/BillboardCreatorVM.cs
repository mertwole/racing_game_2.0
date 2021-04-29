using Editor.Common;
using Editor.GameEntities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.BillboardCreator
{
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bitmap = value as Bitmap;

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class BillboardCreatorVM : INotifyPropertyChanged
    {
        BillboardCreatorModel model = new BillboardCreatorModel();
        public ObservableCollection<LOD> LODs { get => model.LODs; }

        public Visibility DropRegionsVisiblity { get => draggedImage == null ? Visibility.Hidden : Visibility.Visible; }

        UIElement draggedImage;

        public ICommand StartDragImage
        {
            get => new RelayCommand((e) =>
            {
                var args = e as MouseEventArgs;
                
                if(args.LeftButton == MouseButtonState.Pressed)
                {
                    draggedImage = args.Source as UIElement;
                    OnPropertyChanged("DropRegionsVisiblity");

                    DragDrop.DoDragDrop(draggedImage, draggedImage, DragDropEffects.Move);
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

                draggedImage = null;
                OnPropertyChanged("DropRegionsVisiblity");
            });
        }

        public ICommand AddLOD
        {
            get => new RelayCommand((e) =>
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    model.AddLOD(openFileDialog.FileName);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
