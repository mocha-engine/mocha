using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Flask, $"{FontAwesome.Gears} Engine/Demo" )]
internal class DemoWindow : BaseEditorWindow
{
	public override void Draw()
	{
		ImGui.ShowDemoWindow( ref isVisible );
	}
}
