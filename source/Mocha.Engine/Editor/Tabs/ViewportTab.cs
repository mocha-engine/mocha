using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( "Game/Viewport" )]
internal class ViewportTab : BaseTab
{
	public ViewportTab()
	{
		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( "Viewport", ref isVisible );

		var windowSize = ImGui.GetWindowSize() - new System.Numerics.Vector2( 0, 42 );
		EditorHelpers.Image( SceneWorld.Current.Camera.ColorTexture, windowSize );

		ImGui.End();
	}
}
