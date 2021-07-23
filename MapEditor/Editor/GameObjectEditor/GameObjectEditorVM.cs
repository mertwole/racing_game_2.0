using Editor.Common;
using Editor.GameEntities;
using System.Windows.Input;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorVM
    {
        GameObjectEditorModel model = ModelLocator.GetModel<GameObjectEditorModel>();

        public GameObject GameObject { get => model.GameObject; }

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
