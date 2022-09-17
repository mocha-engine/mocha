using ImGuiNET;
using System.ComponentModel;

namespace Mocha.Engine;

[Icon( FontAwesome.Video ), Title( "Scene View" ), Category( "Game" )]
internal class SceneViewWindow : BaseEditorWindow
{
	public SceneViewWindow()
	{
		isVisible = true;
	}

	private void DrawWidgetBar( string[] icons )
	{
		foreach ( var icon in icons )
		{
			ImGuiX.GradientButton( icon );
			ImGui.SameLine();
		}
	}

	public override void Draw()
	{
		ImGui.Begin( "Scene View" );

		var windowSize = ImGui.GetWindowSize() - new System.Numerics.Vector2( 16, 42 );
		ImGuiX.Image( Asset.All.OfType<Texture>().ToList()[1], windowSize );

		SceneWorld.Current.Camera.UpdateAspect( new Point2( (int)windowSize.X, (int)windowSize.Y ) );

		ImGui.SetCursorPos( new Vector2( 24, 48 ) );

		DrawWidgetBar( new[] {
			FontAwesome.ArrowsUpDownLeftRight,
			FontAwesome.Rotate,
			FontAwesome.Maximize
		} );

		ImGui.Dummy( new System.Numerics.Vector2( 8, 0 ) );
		ImGui.SameLine();

		DrawWidgetBar( new[] {
			FontAwesome.BorderAll,
			"  1  "
		} );

		ImGui.Dummy( new System.Numerics.Vector2( 8, 0 ) );
		ImGui.SameLine();

		DrawWidgetBar( new[] {
			FontAwesome.Globe,
			FontAwesome.Cubes,
		} );

		ImGui.End();
	}
}
