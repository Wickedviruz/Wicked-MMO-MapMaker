using Godot;

public partial class EditorController : Node2D
{
	private Camera2D camera;

	public override void _Ready()
	{
		camera = GetNode<Camera2D>("Camera2D");
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion motion && Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			camera.Position -= motion.Relative * camera.Zoom;
		}

		if (e is InputEventMouseButton wheel)
		{
			if (wheel.ButtonIndex == MouseButton.WheelUp && wheel.Pressed)
				camera.Zoom *= 0.9f;

			if (wheel.ButtonIndex == MouseButton.WheelDown && wheel.Pressed)
				camera.Zoom *= 1.1f;
		}
	}
}
