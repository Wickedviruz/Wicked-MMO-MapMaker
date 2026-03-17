using System.Collections.ObjectModel;
using System.Windows.Input;
using MapMaker.Core.Map;

namespace MapMaker.App.ViewModels;

public class LayerPanelViewModel
{
    public ObservableCollection<MapLayer> Layers { get; } = new();
    public MapLayer? SelectedLayer { get; set; }

    public ICommand AddLayerCommand    { get; }
    public ICommand RemoveLayerCommand { get; }
    public ICommand MoveUpCommand      { get; }
    public ICommand MoveDownCommand    { get; }

    public LayerPanelViewModel()
    {
        AddLayerCommand    = new RelayCommand(AddLayer);
        RemoveLayerCommand = new RelayCommand(RemoveLayer);
        MoveUpCommand      = new RelayCommand(MoveUp);
        MoveDownCommand    = new RelayCommand(MoveDown);
    }

    public void Initialize(RMap map)
    {
        Layers.Clear();
        foreach (var layer in map.Layers)
            Layers.Add(layer);
    }

    private void AddLayer()
    {
        var layer = new MapLayer
        {
            Id   = (byte)Layers.Count,
            Name = $"Layer {Layers.Count}",
            Type = LayerType.Objects
        };
        Layers.Add(layer);
        SelectedLayer = layer;
    }

    private void RemoveLayer()
    {
        if (SelectedLayer is null) return;
        Layers.Remove(SelectedLayer);
        SelectedLayer = null;
    }

    private void MoveUp()
    {
        if (SelectedLayer is null) return;
        var idx = Layers.IndexOf(SelectedLayer);
        if (idx <= 0) return;
        Layers.Move(idx, idx - 1);
    }

    private void MoveDown()
    {
        if (SelectedLayer is null) return;
        var idx = Layers.IndexOf(SelectedLayer);
        if (idx < 0 || idx >= Layers.Count - 1) return;
        Layers.Move(idx, idx + 1);
    }
}