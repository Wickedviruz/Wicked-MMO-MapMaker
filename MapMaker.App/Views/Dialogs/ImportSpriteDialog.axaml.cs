using Avalonia.Controls;
using MapMaker.App.ViewModels;

namespace MapMaker.App.Views.Dialogs;

public partial class ImportSpriteDialog : Window
{
    public ImportSpriteDialog()
    {
        InitializeComponent();
        DataContext = new ImportSpriteDialogViewModel(this);
    }
}