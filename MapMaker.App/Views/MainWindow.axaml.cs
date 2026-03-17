using Avalonia.Controls;
using MapMaker.App.ViewModels;

namespace MapMaker.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
    }
}