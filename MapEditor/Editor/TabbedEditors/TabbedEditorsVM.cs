using Editor.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Editor.TabbedEditors
{
    class TabbedEditorsVM : INotifyPropertyChanged
    {
        TabbedEditorsModel model = ModelLocator.GetModel<TabbedEditorsModel>();

        public ObservableCollection<EditorTab> Tabs { get => model.Tabs; }

        EditorTab activeTab;
        public EditorTab ActiveTab { get => activeTab; set => activeTab = value; }

        public TabbedEditorsVM()
        {
            Tabs.CollectionChanged += (s, e) => 
            { 
                if (e.NewItems != null)
                {
                    // Doesn't work if called from current thread.
                    Dispatcher.CurrentDispatcher.BeginInvoke((Action)(
                        () => {
                            activeTab = (EditorTab)e.NewItems[0];
                            OnPropertyChanged("ActiveTab");
                        }
                    ));
                }
            };
        }

        T FindParentOfType<T>(DependencyObject element) where T : class
        {
            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null) return null;
            if (parent is T par) return par;
            return FindParentOfType<T>(parent);
        }

        public ICommand CloseTab
        {
            get => new RelayCommand((e) =>
            {
                var sender = (e as MouseEventArgs).Source;
                var sender_ti = FindParentOfType<TabItem>(sender as DependencyObject);
                var tab_itemscontrol = TabControl.ItemsControlFromItemContainer(sender_ti);
                var tab = tab_itemscontrol.ItemContainerGenerator.ItemFromContainer(sender_ti);

                model.CloseTab(tab as EditorTab);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
