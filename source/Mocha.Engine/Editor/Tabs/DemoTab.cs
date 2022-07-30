using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Flask, $"{FontAwesome.Gears} Engine/Demo Window" )]
internal class DemoTab : BaseTab
{
	public override void Draw()
	{
		ImGui.ShowDemoWindow( ref isVisible );
	}
}
