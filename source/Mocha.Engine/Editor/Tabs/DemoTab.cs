using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( "ImGUI/Demo Window" )]
internal class DemoTab : BaseTab
{
    public override void Draw()
    {
        ImGui.ShowDemoWindow( ref isVisible );
    }
}
