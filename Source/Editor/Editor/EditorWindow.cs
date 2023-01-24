namespace Mocha.Editor;

public abstract class EditorWindow
{
	public bool IsVisible { get; set; } = false;

	public abstract void Draw();
}

