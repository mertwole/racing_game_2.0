using Editor.Common;
using Editor.FileManager;
using Editor.TabbedEditors;
using System.Windows.Input;

namespace Editor
{
    public class MainVM : IViewModel
    {
        MainModel model = new MainModel();

        public void ProvideModelToRequester(RequestModelEventArgs args)
        {
            if (args.Requester is FileManagerVM)
            {
                var file_manager_model = new FileManagerModel(model);
                model.fileManagerModel = file_manager_model;
                args.Requester.SetModel(file_manager_model);
            }
            else if (args.Requester is TabbedEditorsVM)
            {
                var tabbed_editors_model = new TabbedEditorsModel(model);
                model.tabbedEditorsModel = tabbed_editors_model;
                args.Requester.SetModel(tabbed_editors_model);
            }  
        }

        public void SetModel(object model) { }

        public ICommand ApplyChanges
        {
            get => new RelayCommand((e) =>
            {
                if ((e as KeyEventArgs).Key == Key.S && Keyboard.IsKeyDown(Key.LeftCtrl))
                    model.SaveProject();
            });
        }

        public ICommand LoadProject
        {
            get => new RelayCommand((e) =>
            {
                model.LoadProject();
            });
        }
    }
}
