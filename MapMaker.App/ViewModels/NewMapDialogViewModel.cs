using System.Windows.Input;
using Avalonia.Controls;

namespace MapMaker.App.ViewModels;

public class NewMapDialogViewModel
{
    private readonly Window _window;

    public string MapName  { get; set; } = "Untitled";
    public decimal Width    { get; set; } = 100;
    public decimal Height   { get; set; } = 100;
    public decimal TileSize { get; set; } = 32;

    public bool Confirmed { get; private set; }

    public ICommand CreateCommand { get; }
    public ICommand CancelCommand { get; }

    public NewMapDialogViewModel(Window window)
    {
        _window = window;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Create()
    {
        Confirmed = true;
        _window.Close();
    }

    private void Cancel() => _window.Close();
}