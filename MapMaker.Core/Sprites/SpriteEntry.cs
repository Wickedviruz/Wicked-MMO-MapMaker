namespace MapMaker.Core.Sprites;

public enum SpriteCategory
{
    Ground,
    Wall,
    Object,
    Creature,
    Effect
}

public class SpriteEntry
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public SpriteCategory Category { get; set; }
}