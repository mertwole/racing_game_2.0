using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Editor.TrackPreview
{
    public class TrackPreviewModel : INotifyPropertyChanged
    {
        public Bitmap Preview;

        [DllImport("preview_renderer.dll")]
        static unsafe extern void render_preview(
            Byte* data, Int32 data_len,
            Single camera_distance,
            Int32 out_width, Int32 out_height, UInt32* out_pixels
        );

        public static void TrackDataChanged(byte[] rmap_data)
        {
            unsafe
            {
                int width = 1920 / 2;
                int height = 1080 / 2;
                UInt32[] image_data = new UInt32[width * height];
                fixed (Byte* rmap_data_pointer = &rmap_data[0])
                    fixed (UInt32* image_data_pointer = &image_data[0])    
                        render_preview(
                            rmap_data_pointer, rmap_data.Length,
                            0.0f,
                            width, height, image_data_pointer
                        );

                Bitmap img = new Bitmap(width, height);
                for(int x = 0; x < width; x++)
                    for(int y = 0; y < height; y++)
                    {
                        int glob_id = x + y * width;

                        fixed (UInt32* image_data_pointer = &image_data[0])
                        {
                            byte* image_data_pointer_byte = (byte*)image_data_pointer;
                            byte r = image_data_pointer_byte[glob_id * 4 + 0];
                            byte g = image_data_pointer_byte[glob_id * 4 + 1];
                            byte b = image_data_pointer_byte[glob_id * 4 + 2];
                            img.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                        }
                    }

                img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
