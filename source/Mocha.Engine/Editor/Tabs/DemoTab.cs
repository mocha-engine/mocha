using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.VectorSquare} Editor UI/Demo Window" )]
internal class DemoTab : BaseTab
{
	public override void Draw()
	{
		ImGui.ShowDemoWindow( ref isVisible );
	}
}
