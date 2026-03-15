using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class MapSaver : Node
{
	private TilePainter ground;

	public override void _Ready()
	{
		ground = GetNode<TilePainter>("../../Map/Ground");
	}

	public void OnSavePressed()
	{
		GD.Print("OnSavePressed anropad!");
		Save(ground);
	}

	public void Save(TilePainter map)
	{
		GD.Print($"Antal celler att spara: {map.GetUsedCells().Count}");

		var tiles = new List<object>();
		foreach (Vector2I cell in map.GetUsedCells())
		{
			int sourceId = map.GetCellSourceId(cell);
			Vector2I atlasCoords = map.GetCellAtlasCoords(cell);
			tiles.Add(new {
				x = cell.X,
				y = cell.Y,
				sourceId = sourceId,
				atlasX = atlasCoords.X,
				atlasY = atlasCoords.Y
			});
		}

		string json = JsonSerializer.Serialize(tiles);

		// Skapa mappen om den inte finns
		DirAccess.MakeDirRecursiveAbsolute("res://world");

		using var file = FileAccess.Open("res://world/map.json", FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.PrintErr($"Kunde inte öppna filen! Fel: {FileAccess.GetOpenError()}");
			return;
		}

		file.StoreString(json);
		GD.Print("Karta sparad till res://world/map.json");
	}
}
