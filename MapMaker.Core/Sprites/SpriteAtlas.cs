using System.Collections.ObjectModel;

namespace MapMaker.Core.Sprites;

public class SpriteAtlas
{
    public string ImagePath { get; set; } = string.Empty;
    public ObservableCollection<SpriteEntry> Sprites { get; set; } = new();

    public SpriteEntry? GetById(uint id) =>
        Sprites.FirstOrDefault(s => s.Id == id);

    public IEnumerable<SpriteEntry> GetByCategory(SpriteCategory category) =>
        Sprites.Where(s => s.Category == category);
}