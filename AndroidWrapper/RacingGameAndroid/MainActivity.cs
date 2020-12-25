using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Java.Lang;
using System.Runtime.InteropServices;
using Android.Widget;
using Android.Graphics;
using Xamarin.Essentials;
using System;
using Android.Content.PM;
using Android.Views;
using System.Threading;

namespace RacingGameAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity
    {
        [DllImport("core.so")]
        private unsafe static extern void* init_ffi(uint screen_width, uint screen_height);
        [DllImport("core.so")]
        private unsafe static extern void update_ffi(void* game, float delta_time);
        [DllImport("core.so")]
        private unsafe static extern void redraw_ffi(void* game, uint* pixels);

        ImageView screen_image;
        unsafe void* game;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            JavaSystem.LoadLibrary("core");

            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            screen_image = FindViewById<ImageView>(Resource.Id.screen_image);

            screen_image.Post(new Action(Init));
        }

        Bitmap[] renderbuffers;
        int current_renderbuffer;

        void Init()
        {
            int screen_height = (int)DeviceDisplay.MainDisplayInfo.Height / 2;
            int screen_width = (int)DeviceDisplay.MainDisplayInfo.Width / 2;
            renderbuffers = new Bitmap[] {
                Bitmap.CreateBitmap(screen_width, screen_height, Bitmap.Config.Argb8888),
                Bitmap.CreateBitmap(screen_width, screen_height, Bitmap.Config.Argb8888)
            };
            current_renderbuffer = 0;

            unsafe { game = init_ffi((uint)screen_width, (uint)screen_height); }

            ThreadPool.QueueUserWorkItem(o => GameLoop());
        }

        void GameLoop()
        {
            DateTime prev_time = DateTime.Now;
            bool image_set_finished = true;

            while(true)
            {
                DateTime time = DateTime.Now;
                var delta_time = (time - prev_time).TotalSeconds;
                Console.WriteLine(delta_time);
                prev_time = time;

                unsafe { update_ffi(game, (float)delta_time); }

                renderbuffers[current_renderbuffer].EraseColor(0);
                IntPtr renderbuffer_ptr = renderbuffers[current_renderbuffer].LockPixels();
                unsafe { redraw_ffi(game, (uint*)renderbuffer_ptr); }
                renderbuffers[current_renderbuffer].UnlockPixels();

                var prev_renderbuffer = current_renderbuffer;
                current_renderbuffer = 1 - current_renderbuffer;
                
                while(!image_set_finished) { }
                image_set_finished = false;
                RunOnUiThread(() => { screen_image.SetImageBitmap(renderbuffers[prev_renderbuffer]); image_set_finished = true; });
            }
        }
    }
}