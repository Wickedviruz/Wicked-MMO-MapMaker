namespace MapMaker.Core.Maps;

public class MonsterSpawn
{
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int SpawnTime { get; set; } = 60;
}

public class SpawnArea
{
    public int CenterX { get; set; }
    public int CenterY { get; set; }
    public int CenterZ { get; set; }
    public int Radius { get; set; } = 5;
    public List<MonsterSpawn> Monsters { get; set; } = new();
}

public class SpawnData
{
    public List<SpawnArea> Areas { get; set; } = new();

    public SpawnArea AddArea(int centerX, int centerY, int centerZ, int radius = 5)
    {
        var area = new SpawnArea
        {
            CenterX = centerX,
            CenterY = centerY,
            CenterZ = centerZ,
            Radius  = radius
        };
        Areas.Add(area);
        return area;
    }

    public void RemoveArea(SpawnArea area) => Areas.Remove(area);
}