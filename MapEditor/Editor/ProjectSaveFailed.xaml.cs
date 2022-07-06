using System.Windows;

namespace Editor
{
    public partial class ProjectSaveFailed : Window
    {
        public ProjectSaveFailed()
        {
            InitializeComponent();
        }

        private void OkPressed(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
