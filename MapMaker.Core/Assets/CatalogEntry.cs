namespace MapMaker.Core.Assets;

public class CatalogEntry
{
    public string File       { get; set; } = string.Empty;
    public uint   FirstId    { get; set; }
    public uint   LastId     { get; set; }
    public int    TileWidth  { get; set; }
    public int    TileHeight { get; set; }
}

public class Catalog
{
    public int              Version { get; set; } = 1;
    public List<CatalogEntry> Sheets  { get; set; } = new();
}