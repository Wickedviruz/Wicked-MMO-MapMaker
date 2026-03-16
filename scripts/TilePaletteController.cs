using Godot;

public partial class TilePaletteController : Node
{
	[Export] private GridContainer gridContainer;
	[Export] private TilePainter tilePainter;
	private TileSet tileSet;

	public override void _Ready()
	{
		tileSet = tilePainter.TileSet;

		if (tileSet == null)
		{
			GD.PrintErr("TileSet saknas på Ground-noden!");
			return;
		}

		PopulatePalette();
	}

	private void PopulatePalette()
	{
		for (int i = 0; i < tileSet.GetSourceCount(); i++)
		{
			int actualSourceId = tileSet.GetSourceId(i);
			var source = tileSet.GetSource(actualSourceId);

			if (source is TileSetAtlasSource atlasSource)
				AddAtlasTiles(atlasSource, actualSourceId);
		}
	}

	private void AddAtlasTiles(TileSetAtlasSource atlasSource, int sourceId)
	{
		var texture = atlasSource.Texture;
		var tileSize = atlasSource.TextureRegionSize;

		for (int i = 0; i < atlasSource.GetTilesCount(); i++)
		{
			Vector2I atlasCoords = atlasSource.GetTileId(i);

			var atlasTexture = new AtlasTexture();
			atlasTexture.Atlas = texture;
			atlasTexture.Region = new Rect2(
				atlasCoords.X * tileSize.X,
				atlasCoords.Y * tileSize.Y,
				tileSize.X,
				tileSize.Y
			);

			var btn = new TextureButton();
			btn.TextureNormal = atlasTexture;
			btn.CustomMinimumSize = new Vector2(tileSize.X, tileSize.Y);

			int capturedSource = sourceId;
			Vector2I capturedCoords = atlasCoords;

			btn.Pressed += () =>
			{
				tilePainter.SelectedSourceId = capturedSource;
				tilePainter.SelectedAtlasCoords = capturedCoords;
			};

			gridContainer.AddChild(btn);
		}
	}
}
