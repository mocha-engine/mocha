using ImGuiNET;
using System.ComponentModel;

namespace Mocha.Engine;

[Icon( FontAwesome.Gamepad ), Title( "Input" ), Category( "Engine" )]
internal class InputWindow : BaseEditorWindow
{
	public override void Draw()
	{
		ImGui.Begin( "Input", ref isVisible );

		ImGuiX.Title(
			$"{FontAwesome.Gamepad} Input",
			"This is where you can see things like input buttons and mouse info."
		);

		ImGui.Text( $"{Input.Snapshot}" );

		ImGui.End();
	}
}
