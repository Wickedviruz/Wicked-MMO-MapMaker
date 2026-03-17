namespace MapMaker.Core.Map;

public class RMap
{
    public string Name { get; set; } = "Untitled";
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileSize { get; set; } = 32;
    public List<MapLayer> Layers { get; set; } = new();

    public MapLayer AddLayer(string name, LayerType type)
    {
        var layer = new MapLayer
        {
            Id = (byte)Layers.Count,
            Name = name,
            Type = type
        };
        Layers.Add(layer);
        return layer;
    }

    public MapLayer? GetLayer(byte id) =>
        Layers.FirstOrDefault(l => l.Id == id);
}