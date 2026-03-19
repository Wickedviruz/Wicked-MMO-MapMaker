using Avalonia.Controls;
using MapMaker.App.ViewModels;
using MapMaker.App.Views.Controls;

namespace MapMaker.App.Views.Workspace;

public partial class MapWorkspace : UserControl
{
    public MapWorkspace()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var vm = new MapWorkspaceViewModel();
        DataContext = vm;

        var canvas = this.FindControl<MapCanvas>("Canvas");
        if (canvas is not null)
            canvas.ViewModel = vm;
    }
}