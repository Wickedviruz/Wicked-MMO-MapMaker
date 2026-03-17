# MapMaker — Projektstruktur

## Solution

```
MapMaker.sln
MapMaker.App        # Avalonia UI
MapMaker.Core       # All logik, inga UI-beroenden
```

---

## MapMaker.Core

```
MapMaker.Core/

Sprites/
   SpriteEntry.cs          ✅ Klar — id, x, y, w, h, category, name
   SpriteAtlas.cs          ✅ Klar — håller lista av SpriteEntry + imagePath
   SpriteImporter.cs       Importera enskild PNG eller sprite sheet
                                  - Enskild PNG ett ID
                                  - Sheet ange tileW, tileH, cols, rows auto-ID

Items/
   ItemFlags.cs            [Flags] enum med alla flaggor
                                 Walkable, BlockSolid, BlockMissiles,
                                 BlockPathfinder, Pickupable, Stackable,
                                 Movable, Rotatable, Hangable,
                                 HookSouth, HookEast, Readable,
                                 MultiUse, ForceUse, FullGround,
                                 HasElevation, IgnoreLook
   
   ItemDefinition.cs       Datamodell
                                 uint Id
                                 uint SpriteId
                                 string Name
                                 ItemFlags Flags
                                 ItemType Type (None, Weapon, Armor, Container, ...)
                                 StackOrder StackOrder (None, 1, 2, 3)
                                 byte LightLevel
                                 byte LightColor
                                 uint MinimapColor
                                 ushort WareId
                                 ushort ReadLength
                                 ushort WriteLength
   
   ItemDefSerializer.cs    Spara/ladda items.wdef (binärt)
                                 Magic bytes: "WDEF" + version
                                 En post per item
   
   ItemXmlSerializer.cs    Spara/ladda items.xml (server-side)
                                  <items>
                                    <item id="1" name="Grass">
                                      <attribute key="description" value="..." />
                                      <attribute key="weight" value="0" />
                                      <attribute key="type" value="ground" />
                                    </item>
                                  </items>

Maps/
    RMap.cs                 ✅ Klar — Name, Width, Height, TileSize, Layers
    MapLayer.cs             ✅ Klar — Id, Name, Type, Visible, Tiles dict
    LayerTile.cs            ✅ Klar — SpriteId, Flags
    
    SpawnData.cs            Datamodell för spawns
                                  class SpawnArea
                                    int CenterX, CenterY, CenterZ
                                    int Radius
                                    List<MonsterSpawn> Monsters
                                  class MonsterSpawn
                                    string Name
                                    int X, Y, Z
                                    int SpawnTime
    
    SpawnSerializer.cs      Läs/skriv XML
                                  Samma format som TFS:
                                  <spawns>
                                    <spawn centerx="" centery="" centerz="" radius="">
                                      <monster name="" x="" y="" z="" spawntime="" />
                                    </spawn>
                                  </spawns>
    
    MapSerializer.cs        Spara/ladda .wmap (binärt)
                                   Döper om/ersätter RMapSerializer.cs
                                   Magic bytes: "WMAP" + version
                                   Header: namn, bredd, höjd, tileSize
                                   Lager + tiles
                                   Sparar även sökväg till spawn-XML automatiskt
```

---

## MapMaker.App

```
MapMaker.App/

Views/
   MainWindow.axaml        ✅ Klar — grundlayout, toolbar, tre kolumner
   
   Panels/
      SpritePickerPanel.axaml     Visar sprites grupperade per kategori
      LayerPanel.axaml            Lagerhantering (lägg till, ta bort, byt ordning)
      SpawnPanel.axaml            Lista spawns, lägg till/ta bort monster
   
   Canvas/
      MapCanvas.axaml             Custom Avalonia control, renderar tiles
   
   Dialogs/
       NewMapDialog.axaml          Namn, bredd, höjd, tileSize
       ImportSpriteDialog.axaml    Välj PNG, enskild eller sheet
                                         Om sheet: ange tileW, tileH, cols, rows
       ItemEditorDialog.axaml      Redigera item: sprite, flaggor, attribut

ViewModels/
   MainViewModel.cs                ✅ Klar (grund) — New/Open/Save, lagerhantering
   SpritePickerViewModel.cs        Håller laddad SpriteAtlas, filtrering
   LayerViewModel.cs               Wraps MapLayer, INotifyPropertyChanged
   SpawnViewModel.cs               Håller SpawnData, CRUD för spawns
   ItemEditorViewModel.cs          Redigera ItemDefinition

RelayCommand.cs                     ✅ Klar
```

---

## Filformat — Översikt

| Fil | Format | Används av |
|-----|--------|------------|
| `assets.wsp` | Binärt | Editor + Client (sprites) |
| `items.wdef` | Binärt | Editor + Server + Client (flaggor) |
| `items.xml` | XML | Server (namn, gameplay-attribut) |
| `mymap.wmap` | Binärt | Editor + Server (tile-data) |
| `mymap.xml` | XML | Editor + Server (spawn-data) |

---

## Build-ordning

```
1. Items/ItemFlags.cs
2. Items/ItemDefinition.cs
3. Items/ItemDefSerializer.cs
4. Items/ItemXmlSerializer.cs
5. Sprites/SpriteImporter.cs
6. Maps/SpawnData.cs
7. Maps/SpawnSerializer.cs
8. Maps/MapSerializer.cs          (ersätter RMapSerializer)
9. App: Dialogs + Panels + Canvas
```