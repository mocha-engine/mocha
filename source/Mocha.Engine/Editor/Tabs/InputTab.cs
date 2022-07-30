using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Gamepad, $"{FontAwesome.Gears} Engine/Input" )]
internal class InputTab : BaseTab
{
	public override void Draw()
	{
		ImGui.Begin( "Input", ref isVisible );

		EditorHelpers.Title(
			$"{FontAwesome.Gamepad} Input",
			"This is where you can see things like input buttons and mouse info."
		);

		ImGui.Text( $"{Input.Snapshot}" );

		ImGui.End();
	}
}
