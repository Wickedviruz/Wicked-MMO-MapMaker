using Godot;

public partial class TilePainter : TileMapLayer
{
	public int SelectedSourceId { get; set; } = 0;
	public Vector2I SelectedAtlasCoords { get; set; } = Vector2I.Zero;

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseButton mouse
			&& mouse.Pressed
			&& mouse.ButtonIndex == MouseButton.Left)
		{
			// Blockera om musen är över något UI-element
			if (GetViewport().GuiGetHoveredControl() != null)
				return;

			Vector2 worldPos = GetGlobalMousePosition();
			Vector2I cell = LocalToMap(ToLocal(worldPos));
			SetCell(cell, SelectedSourceId, SelectedAtlasCoords);
		}
	}
}
