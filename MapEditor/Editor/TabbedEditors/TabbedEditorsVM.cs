using Editor.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor.TabbedEditors
{
    class TabbedEditorsVM
    {
        TabbedEditorsModel model = ModelLocator.GetModel<TabbedEditorsModel>();

        public ObservableCollection<EditorTab> Tabs { get => model.Tabs; }

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
    }
}
