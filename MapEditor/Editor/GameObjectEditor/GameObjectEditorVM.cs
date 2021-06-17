using Editor.CustomControls;
using System.Windows;

namespace Editor.GameObjectEditor
{
    public class GameObjectEditorVM
    {
        GameObjectEditorModel model = ModelLocator.GetModel<GameObjectEditorModel>();

        // Front view grid
        public static readonly DependencyProperty FrontViewGridProperty =
        DependencyProperty.RegisterAttached(
        "FrontViewGrid", typeof(InfiniteGridView),
        typeof(GameObjectEditorVM), new FrameworkPropertyMetadata(OnFrontViewGridChanged));

        public static void SetFrontViewGrid(DependencyObject element, InfiniteGridView value) => element.SetValue(FrontViewGridProperty, value);
        public static InfiniteGridView GetFrontViewGrid(DependencyObject element) => (InfiniteGridView)element.GetValue(FrontViewGridProperty);

        static InfiniteGridView frontViewGrid = null;
        public static void OnFrontViewGridChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args) => frontViewGrid = obj as InfiniteGridView;

        // Left view grid
        public static readonly DependencyProperty LeftViewGridProperty =
        DependencyProperty.RegisterAttached(
        "LeftViewGrid", typeof(InfiniteGridView),
        typeof(GameObjectEditorVM), new FrameworkPropertyMetadata(OnLeftViewGridChanged));

        public static void SetLeftViewGrid(DependencyObject element, InfiniteGridView value) => element.SetValue(LeftViewGridProperty, value);
        public static InfiniteGridView GetLeftViewGrid(DependencyObject element) => (InfiniteGridView)element.GetValue(LeftViewGridProperty);

        static InfiniteGridView leftViewGrid = null;
        public static void OnLeftViewGridChanged
        (DependencyObject obj, DependencyPropertyChangedEventArgs args) => leftViewGrid = obj as InfiniteGridView;
    }
}
