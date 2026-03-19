using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MapMaker.App.State;
using MapMaker.Core.Map;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class MapWorkspaceViewModel : INotifyPropertyChanged
{
    private bool   _showGrid    = true;
    private int    _brushSize   = 1;
    private string _zoomLabel   = "100%";
    private int?   _cursorTileX;
    private int?   _cursorTileY;
    private string _cursorPosition = "X: 0, Y: 0";
    private MapLayer? _activeLayer;
    private SpriteEntry? _selectedSprite;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public bool ShowGrid
    {
        get => _showGrid;
        set { _showGrid = value; OnPropertyChanged(); }
    }

    public int BrushSize
    {
        get => _brushSize;
        set { _brushSize = Math.Clamp(value, 1, 10); OnPropertyChanged(); }
    }

    public string ZoomLabel
    {
        get => _zoomLabel;
        set { _zoomLabel = value; OnPropertyChanged(); }
    }

    public int? CursorTileX
    {
        get => _cursorTileX;
        set { _cursorTileX = value; OnPropertyChanged(); }
    }

    public int? CursorTileY
    {
        get => _cursorTileY;
        set { _cursorTileY = value; OnPropertyChanged(); }
    }

    public string CursorPosition
    {
        get => _cursorPosition;
        set { _cursorPosition = value; OnPropertyChanged(); }
    }

    public MapLayer? ActiveLayer
    {
        get => _activeLayer;
        set { _activeLayer = value; OnPropertyChanged(); }
    }

    public uint? SelectedSpriteId => _selectedSprite?.Id;

    public SpriteEntry? SelectedSprite
    {
        get => _selectedSprite;
        set { _selectedSprite = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedSpriteId)); }
    }

    // Lager från kartan
    public ObservableCollection<MapLayer> Layers =>
        EditorSession.Current.CurrentMap is not null
            ? new ObservableCollection<MapLayer>(EditorSession.Current.CurrentMap.Layers)
            : new ObservableCollection<MapLayer>();

    // Sprites från atlas
    public ObservableCollection<SpriteEntry> Sprites =>
        EditorSession.Current.Atlas.Sprites;

    public ICommand ToggleGridCommand     { get; }
    public ICommand IncreaseBrushCommand  { get; }
    public ICommand DecreaseBrushCommand  { get; }

    public MapWorkspaceViewModel()
    {
        ToggleGridCommand    = new RelayCommand(() => ShowGrid = !ShowGrid);
        IncreaseBrushCommand = new RelayCommand(() => BrushSize++);
        DecreaseBrushCommand = new RelayCommand(() => BrushSize--);

        EditorSession.Current.AtlasChanged += () =>
            OnPropertyChanged(nameof(Sprites));
    }

    public void OnMapLoaded()
    {
        OnPropertyChanged(nameof(Layers));
        ActiveLayer = EditorSession.Current.CurrentMap?.Layers.FirstOrDefault();
    }
}