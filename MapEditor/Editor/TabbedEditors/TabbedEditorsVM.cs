using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.TabbedEditors
{
    class TabbedEditorsVM
    {
        TabbedEditorsModel model = ModelLocator.GetModel<TabbedEditorsModel>();

        public ObservableCollection<EditorTab> Tabs { get => model.Tabs; }
    }
}
