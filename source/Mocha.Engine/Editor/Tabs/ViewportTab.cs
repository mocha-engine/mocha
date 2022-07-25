using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.Gamepad} Game/Viewport" )]
internal class ViewportTab : BaseTab
{
	public ViewportTab()
	{
		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( "Viewport" );

		var windowSize = ImGui.GetWindowSize() - new System.Numerics.Vector2( 16, 42 );
		EditorHelpers.Image( SceneWorld.Current.Camera.ColorTexture, windowSize );

		SceneWorld.Current.Camera.UpdateAspect( new Point2( (int)windowSize.X, (int)windowSize.Y ) );

		ImGui.End();
	}
}
