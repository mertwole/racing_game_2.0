using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Editor.Common
{
    public class ViewBehaviour : Behavior<UserControl>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += (s, e) =>
                AssociatedObject.RaiseEvent(new RequestModelEventArgs(this, AssociatedObject.DataContext as IViewModel));

            RequestModel.AddRequestModelHandler(
                AssociatedObject,
                (s, e) =>
                    (AssociatedObject.DataContext as IViewModel).ProvideModelToRequester(e as RequestModelEventArgs)
            );
        }
    }
}
