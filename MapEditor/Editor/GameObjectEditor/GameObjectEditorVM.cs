using Editor.Common;
using Editor.GameEntities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.GameObjectEditor
{
    public class EntityDataTemplateSelector : DataTemplateSelector
    {
        DataTemplate billboardDataTemplate;
        public DataTemplate BillboardDataTemplate { get => billboardDataTemplate; set => billboardDataTemplate = value; }

        DataTemplate colliderDataTemplate;
        public DataTemplate ColliderDataTemplate { get => colliderDataTemplate; set => colliderDataTemplate = value; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Billboard)
                return billboardDataTemplate;

            if(item is Collider)
                return colliderDataTemplate;

            throw new System.Exception("Wrong data type : expected Billboard or Collider.");
        }
    }

    public class GameObjectEditorVM
    {
        GameObjectEditorModel model = ModelLocator.GetModel<GameObjectEditorModel>();

        public GameObject GameObject { get => model.GameObject; }

        ObservableCollection<object> billboardsAndColliders = new ObservableCollection<object>();
        public ObservableCollection<object> BillboardsAndColliders { get => billboardsAndColliders; }

        public GameObjectEditorVM()
        {
            foreach (var billboard in GameObject.Billboards)
                billboardsAndColliders.Add(billboard);
            foreach (var collider in GameObject.Colliders)
                billboardsAndColliders.Add(collider);
        }

        public ICommand ApplyChanges
        {
            get => new RelayCommand((e) =>
            {
                var args = e as KeyboardEventArgs;

                if ((e as KeyEventArgs).Key == Key.S && args.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                    model.ApplyChanges();
            });
        }
    }
}
