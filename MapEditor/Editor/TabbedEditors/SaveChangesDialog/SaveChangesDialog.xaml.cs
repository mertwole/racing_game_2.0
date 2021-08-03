using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Editor.TabbedEditors.SaveChangesDialog
{
    public partial class SaveChangesDialog : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public enum Choise
        {
            Keep,
            Discard,
            ContinueEditing
        }

        public Choise choise;

        public SaveChangesDialog()
        {
            Loaded += (s, e) => RemoveCloseButton();

            InitializeComponent();
        }

        void RemoveCloseButton()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void KeepSelected(object sender, RoutedEventArgs e)
        {
            choise = Choise.Keep;
            Close();
        }

        private void DiscardSelected(object sender, RoutedEventArgs e)
        {
            choise = Choise.Discard;
            Close();
        }

        private void ContinueEditingSelected(object sender, RoutedEventArgs e)
        {
            choise = Choise.ContinueEditing;
            Close();
        }
    }
}
