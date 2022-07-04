using Editor.GameEntities;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Editor.TrackEditor.TrackPreview
{
    public class TrackPreviewModel : INotifyPropertyChanged
    {
        Track track;

        Bitmap preview;
        public Bitmap Preview { get => preview; }

        double latestPointerPosition = 0;

        [DllImport("preview_renderer.dll")]
        static unsafe extern void render_preview(
            Byte* data, Int32 data_len,
            Single camera_distance,
            Int32 out_width, Int32 out_height, UInt32* out_pixels
        );

        public TrackPreviewModel(TrackEditorModel model)
        {
            preview = new Bitmap(192, 108);
            track = model.Track;

            model.PropertyChanged += (s, e) => { 
                if (e.PropertyName == "PointerPositionNormalized") 
                { 
                    latestPointerPosition = model.PointerPositionNormalized; 
                    UpdatePreview(); 
                } 
            };
            track.PropertyChanged += (s, e) => UpdatePreview();

            UpdatePreview();
        }

        static Serializers serializer = new Serializers();
        void UpdatePreview()
        {
            var serialized = serializer.SerializeRmapSingleTrack(track);
            serialized.Seek(0, SeekOrigin.Begin);
            var rmap_data = serialized.ToArray();
            Update(rmap_data, (float)latestPointerPosition * (float)track.Parameters.Length);
        }

        void Update(byte[] rmap_data, float camera_distance)
        {
            if (preview == null)
                return;

            var bmp_data = preview.LockBits(
                    new Rectangle(0, 0, preview.Width, preview.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            unsafe
            {
                fixed (Byte* rmap_data_pointer = &rmap_data[0])
                {
                    render_preview(
                        rmap_data_pointer, rmap_data.Length, camera_distance,
                        preview.Width, preview.Height, (UInt32*)bmp_data.Scan0.ToPointer()
                    );
                }
            }
            
            preview.UnlockBits(bmp_data);
            preview.RotateFlip(RotateFlipType.RotateNoneFlipY);
            preview = SwapRedAndBlueChannels(preview);

            OnPropertyChanged("Preview");
        }

        Bitmap SwapRedAndBlueChannels(Bitmap bitmap)
        {
            var imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(new ColorMatrix(
                new[]
                    {
                        new[] {0.0F, 0.0F, 1.0F, 0.0F, 0.0F},
                        new[] {0.0F, 1.0F, 0.0F, 0.0F, 0.0F},
                        new[] {1.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                        new[] {0.0F, 0.0F, 0.0F, 1.0F, 0.0F},
                        new[] {0.0F, 0.0F, 0.0F, 0.0F, 1.0F}
                    }
                ));

            var temp = new Bitmap(bitmap.Width, bitmap.Height);
            GraphicsUnit pixel = GraphicsUnit.Pixel;

            using (Graphics g = Graphics.FromImage(temp))
                g.DrawImage(bitmap, Rectangle.Round(bitmap.GetBounds(ref pixel)), 
                            0, 0, bitmap.Width, bitmap.Height,
                            GraphicsUnit.Pixel, imageAttr);

            return temp;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
