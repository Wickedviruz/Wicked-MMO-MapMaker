using System;
using System.Windows.Input;
using Avalonia.Controls;
using MapMaker.Core.Maps;
using MapMaker.Core.Map;
using MapMaker.App.State;

namespace MapMaker.App.ViewModels;

public class MainViewModel
{
    private readonly Window _mainWindow;

    // Status bar
    public string StatusText     { get; set; } = "Redo.";
    public string CursorPosition { get; set; } = "X: 0, Y: 0";
    public string MapInfo        { get; set; } = "Ingen karta laddad";
    public string ZoomLabel      { get; set; } = "100%";

    // Sub-ViewModels
    public SpritePickerViewModel SpritePicker { get; } = new();
    public LayerPanelViewModel   LayerPanel   { get; } = new();
    public SpawnPanelViewModel   SpawnPanel   { get; } = new();
    public ItemPanelViewModel    ItemPanel    { get; } = new();
    public WorkspaceViewModel   Workspace     { get; } = new(); 
    public SpriteWorkspaceViewModel SpriteWorkspace { get; }
    public ItemWorkspaceViewModel   ItemWorkspace   { get; }  
    public MapWorkspaceViewModel MapWorkspace { get; } = new();

    // Commands
    public ICommand NewMapCommand        { get; }
    public ICommand OpenMapCommand       { get; }
    public ICommand SaveMapCommand       { get; }
    public ICommand SaveAsMapCommand     { get; }
    public ICommand ImportSpritesCommand { get; }
    public ICommand ExitCommand          { get; }
    public ICommand UndoCommand          { get; }
    public ICommand RedoCommand          { get; }
    public ICommand ToggleGridCommand    { get; }
    public ICommand ToggleSpawnsCommand  { get; }
    public ICommand SetToolDrawCommand   { get; }
    public ICommand SetToolEraseCommand  { get; }
    public ICommand SetToolFillCommand   { get; }

    private RMap? _currentMap;

    public MainViewModel(Window mainWindow)
    {
        _mainWindow         = mainWindow;
        SpriteWorkspace  = new SpriteWorkspaceViewModel(mainWindow);
        ItemWorkspace    = new ItemWorkspaceViewModel(mainWindow);
        NewMapCommand       = new RelayCommand(NewMap);
        OpenMapCommand      = new RelayCommand(OpenMap);
        SaveMapCommand      = new RelayCommand(SaveMap);
        SaveAsMapCommand    = new RelayCommand(SaveAsMap);
        ImportSpritesCommand = new RelayCommand(ImportSprites);
        ExitCommand         = new RelayCommand(Exit);
        UndoCommand         = new RelayCommand(Undo);
        RedoCommand         = new RelayCommand(Redo);
        ToggleGridCommand   = new RelayCommand(ToggleGrid);
        ToggleSpawnsCommand = new RelayCommand(ToggleSpawns);
        SetToolDrawCommand  = new RelayCommand(() => StatusText = "Verktyg: Rita");
        SetToolEraseCommand = new RelayCommand(() => StatusText = "Verktyg: Radera");
        SetToolFillCommand  = new RelayCommand(() => StatusText = "Verktyg: Fyll");
    }

    private async void NewMap()
    {
        var dialog = new Views.Dialogs.NewMapDialog();
        await dialog.ShowDialog(_mainWindow);

        var vm = (NewMapDialogViewModel)dialog.DataContext!;
        if (!vm.Confirmed) return;

        _currentMap = new RMap
        {
            Name     = vm.MapName,
            Width    = (int)vm.Width,
            Height   = (int)vm.Height,
            TileSize = (int)vm.TileSize
        };
        _currentMap.AddLayer("Ground", LayerType.Ground);
        LayerPanel.Initialize(_currentMap);
        SpawnPanel.Initialize(new SpawnData());
        EditorSession.Current.CurrentMap = _currentMap;
        MapWorkspace.OnMapLoaded();
        MapInfo    = $"{_currentMap.Name}  {_currentMap.Width}x{_currentMap.Height}";
        StatusText = "Ny karta skapad.";
    }

    private async void ImportSprites()
    {
        var dialog = new Views.Dialogs.ImportSpriteDialog();
        await dialog.ShowDialog(_mainWindow);

        var vm = (ImportSpriteDialogViewModel)dialog.DataContext!;
        if (!vm.Confirmed) return;

        foreach (var sprite in vm.ImportedSprites)
            EditorSession.Current.Atlas.Sprites.Add(sprite);

        EditorSession.Current.MarkDirty();
        StatusText = $"{vm.ImportedSprites.Count} sprites importerade.";
    }

    private void OpenMap()      { StatusText = "Öppna — ej implementerat än."; }
    private void SaveMap()      { StatusText = "Spara — ej implementerat än."; }
    private void SaveAsMap()    { StatusText = "Spara som — ej implementerat än."; }
    private void Exit()         => Environment.Exit(0);
    private void Undo()         { StatusText = "Ångra — ej implementerat än."; }
    private void Redo()         { StatusText = "Gör om — ej implementerat än."; }
    private void ToggleGrid()   { StatusText = "Grid togglad."; }
    private void ToggleSpawns() { StatusText = "Spawns togglad."; }
}