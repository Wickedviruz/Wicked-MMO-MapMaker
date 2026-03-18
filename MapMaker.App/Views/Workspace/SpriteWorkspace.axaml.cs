using Avalonia.Controls;
using MapMaker.App.ViewModels;

namespace MapMaker.App.Views.Workspace;

public partial class SpriteWorkspace : UserControl
{
    public SpriteWorkspace()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var window = this.VisualRoot as Window;
        if (window is not null)
            DataContext = new SpriteWorkspaceViewModel(window);
    }
}