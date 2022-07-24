using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( "Debug/Input" )]
internal class InputTab : BaseTab
{
	public override void Draw()
	{
		ImGui.Begin( "Input", ref isVisible );

		ImGui.Text( $"{Input.Snapshot}");

		ImGui.End();
	}
}
