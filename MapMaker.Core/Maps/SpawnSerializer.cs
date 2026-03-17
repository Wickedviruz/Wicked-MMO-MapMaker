using System.Xml.Linq;

namespace MapMaker.Core.Maps;

public static class SpawnSerializer
{
    // Spawn-filen får samma namn som kartan men med .xml
    public static string GetSpawnPath(string mapPath) =>
        Path.ChangeExtension(mapPath, ".xml");

    public static void Save(SpawnData data, string mapPath)
    {
        var spawnPath = GetSpawnPath(mapPath);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("spawns",
                data.Areas.Select(area =>
                    new XElement("spawn",
                        new XAttribute("centerx", area.CenterX),
                        new XAttribute("centery", area.CenterY),
                        new XAttribute("centerz", area.CenterZ),
                        new XAttribute("radius", area.Radius),
                        area.Monsters.Select(m =>
                            new XElement("monster",
                                new XAttribute("name", m.Name),
                                new XAttribute("x", m.X),
                                new XAttribute("y", m.Y),
                                new XAttribute("z", m.Z),
                                new XAttribute("spawntime", m.SpawnTime)
                            )
                        )
                    )
                )
            )
        );

        doc.Save(spawnPath);
    }

    public static SpawnData Load(string mapPath)
    {
        var spawnPath = GetSpawnPath(mapPath);

        if (!File.Exists(spawnPath))
            return new SpawnData();

        var doc = XDocument.Load(spawnPath);
        var data = new SpawnData();

        foreach (var spawnEl in doc.Root!.Elements("spawn"))
        {
            var area = new SpawnArea
            {
                CenterX = (int)spawnEl.Attribute("centerx")!,
                CenterY = (int)spawnEl.Attribute("centery")!,
                CenterZ = (int)spawnEl.Attribute("centerz")!,
                Radius  = (int)spawnEl.Attribute("radius")!
            };

            foreach (var monsterEl in spawnEl.Elements("monster"))
            {
                area.Monsters.Add(new MonsterSpawn
                {
                    Name      = (string)monsterEl.Attribute("name")!,
                    X         = (int)monsterEl.Attribute("x")!,
                    Y         = (int)monsterEl.Attribute("y")!,
                    Z         = (int)monsterEl.Attribute("z")!,
                    SpawnTime = (int)monsterEl.Attribute("spawntime")!
                });
            }

            data.Areas.Add(area);
        }

        return data;
    }
}