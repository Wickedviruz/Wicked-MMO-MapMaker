using Avalonia.Controls;
using MapMaker.App.ViewModels;

namespace MapMaker.App.Views.Dialogs;

public partial class NewMapDialog : Window
{
    public NewMapDialog()
    {
        InitializeComponent();
        DataContext = new NewMapDialogViewModel(this);
    }
}