﻿using System.Windows;

namespace Editor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadProject(object sender, RoutedEventArgs e)
        {
            MainModel.LoadProject();
        }
    }
}
