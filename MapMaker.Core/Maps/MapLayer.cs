namespace MapMaker.Core.Map;

public enum LayerType
{
    Ground,
    Objects,
    Overlay,
    Spawn,
    Logic
}

public class MapLayer
{
    public byte Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LayerType Type { get; set; }
    public bool Visible { get; set; } = true;
    public Dictionary<(int X, int Y), LayerTile> Tiles { get; set; } = new();

    public void SetTile(int x, int y, uint spriteId, byte flags = 0)
    {
        Tiles[(x, y)] = new LayerTile { SpriteId = spriteId, Flags = flags };
    }

    public void RemoveTile(int x, int y)
    {
        Tiles.Remove((x, y));
    }

    public LayerTile? GetTile(int x, int y)
    {
        Tiles.TryGetValue((x, y), out var tile);
        return tile;
    }
}