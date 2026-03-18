using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MapMaker.App.ViewModels;

public enum WorkspaceMode { Sprites, Items, Map }

public class WorkspaceViewModel : INotifyPropertyChanged
{
    private WorkspaceMode _mode = WorkspaceMode.Sprites;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public WorkspaceMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSprites));
            OnPropertyChanged(nameof(IsItems));
            OnPropertyChanged(nameof(IsMap));
        }
    }

    public bool IsSprites => Mode == WorkspaceMode.Sprites;
    public bool IsItems   => Mode == WorkspaceMode.Items;
    public bool IsMap     => Mode == WorkspaceMode.Map;

    public ICommand SetSpritesCommand { get; }
    public ICommand SetItemsCommand   { get; }
    public ICommand SetMapCommand     { get; }

    public WorkspaceViewModel()
    {
        SetSpritesCommand = new RelayCommand(() => Mode = WorkspaceMode.Sprites);
        SetItemsCommand   = new RelayCommand(() => Mode = WorkspaceMode.Items);
        SetMapCommand     = new RelayCommand(() => Mode = WorkspaceMode.Map);
    }

    
}