using Avalonia.Controls;
using MapMaker.App.ViewModels;

namespace MapMaker.App.Views.Workspace;

public partial class ItemWorkspace : UserControl
{
    private bool _initialized = false;

    public ItemWorkspace()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_initialized) return;

        var window = this.VisualRoot as Window;
        if (window is null) return;

        DataContext = new ItemWorkspaceViewModel(window);
        _initialized = true;
    }
}