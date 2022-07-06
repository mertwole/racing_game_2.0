namespace Editor
{
    public interface IViewModel
    {
        void SetModel(object model);
        void ProvideModelToRequester(RequestModelEventArgs args);
    }
}
