using ImGuiNET;
using System.ComponentModel;

namespace Mocha.Engine;

[Icon( FontAwesome.Flask ), Title( "Demo" ), Category( "Engine" )]
internal class DemoWindow : BaseEditorWindow
{
	public override void Draw()
	{
		ImGui.ShowDemoWindow( ref isVisible );
	}
}
