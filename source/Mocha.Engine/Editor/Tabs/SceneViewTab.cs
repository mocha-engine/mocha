using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Video, $"{FontAwesome.Gamepad} Game/Scene View" )]
internal class SceneViewTab : BaseTab
{
	public SceneViewTab()
	{
		isVisible = true;
	}

	private void DrawWidgetBar( string[] icons )
	{
		var drawList = ImGui.GetWindowDrawList();
		var windowPos = ImGui.GetWindowPos();

		var padding = new System.Numerics.Vector2( 16, 16 );

		var p0 = windowPos + ImGui.GetCursorPos() - (padding * 0.5f);
		var p1 = p0 + ImGui.CalcTextSize( String.Join( "     ", icons ) ) + (padding);
		var col = ImGui.GetColorU32( ImGuiCol.WindowBg );
		drawList.AddRectFilled( p0, p1, col, 5f );

		var p2 = p0 + ImGui.CalcTextSize( icons[0] ) + padding;
		var col1 = ImGui.GetColorU32( ImGuiCol.Button );
		drawList.AddRectFilled( p0, p2, col1, 4f );

		ImGui.Text( String.Join( "     ", icons ) );
		ImGui.SameLine();
		ImGui.Dummy( padding );
	}

	public override void Draw()
	{
		ImGui.Begin( "Scene View" );

		var windowSize = ImGui.GetWindowSize() - new System.Numerics.Vector2( 16, 42 );
		EditorHelpers.Image( Asset.All.OfType<Texture>().ToList()[1], windowSize );

		SceneWorld.Current.Camera.UpdateAspect( new Point2( (int)windowSize.X, (int)windowSize.Y ) );

		ImGui.SetCursorPos( new Vector2( 24, 48 ) );

		DrawWidgetBar( new[] {
			FontAwesome.ArrowsUpDownLeftRight,
			FontAwesome.Rotate,
			FontAwesome.Maximize
		} );

		ImGui.SameLine();

		DrawWidgetBar( new[] {
			FontAwesome.BorderAll,
			"  1  "
		} );

		ImGui.SameLine();

		DrawWidgetBar( new[] {
			FontAwesome.Globe,
			FontAwesome.Cubes,
		} );

		ImGui.Text( "TODO" );

		ImGui.End();
	}
}
