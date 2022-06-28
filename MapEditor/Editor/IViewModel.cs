namespace Editor
{
    public interface IViewModel
    {
        void ProvideModelToRequester(RequestModelEventArgs args);
        void SetModel(object model);
    }
}
