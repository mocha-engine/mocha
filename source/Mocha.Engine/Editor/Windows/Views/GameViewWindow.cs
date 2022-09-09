using ImGuiNET;
using System.ComponentModel;

namespace Mocha.Engine;

[Icon( FontAwesome.Gamepad ), Title( "Game View" ), Category( "Game" )]
internal class GameViewWindow : BaseEditorWindow
{
	public GameViewWindow()
	{
		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( "Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse );

		var windowSize = ImGui.GetWindowSize() - new System.Numerics.Vector2( 16, 16 );
		var tint = (World.Current.State == World.States.Paused) ? new Vector4( 0.5f, 0.5f, 0.5f, 1.0f ) : Vector4.One;

		ImGuiX.Image( SceneWorld.Current.Camera.ColorTexture, windowSize, tint );

		SceneWorld.Current.Camera.UpdateAspect( new Point2( (int)windowSize.X, (int)windowSize.Y ) );

		if ( World.Current.State == World.States.Paused )
		{
			ImGui.PushFont( Editor.HeadingFont );

			var drawList = ImGui.GetWindowDrawList();
			var center = ImGui.GetWindowPos() + (windowSize / 2.0f) + new System.Numerics.Vector2( 0, 32 );

			var text = $"{FontAwesome.Pause} Game paused.";
			var textSize = ImGui.CalcTextSize( text );
			var textCol = ImGui.GetColorU32( ImGuiCol.Text );

			var padding = new System.Numerics.Vector2( 32, 16 );
			var bgCol = ImGui.GetColorU32( ImGuiCol.WindowBg, 0.5f );
			drawList.AddRectFilled( center - (textSize / 2.0f) - padding, center + (textSize / 2.0f) + padding, bgCol, 5f );

			drawList.AddText( center - (textSize / 2.0f), textCol, text );

			ImGui.PopFont();
		}

		ImGui.End();
	}
}
