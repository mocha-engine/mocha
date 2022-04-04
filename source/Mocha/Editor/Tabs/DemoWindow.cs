﻿using ImGuiNET;

namespace Mocha;

[EditorMenu( "ImGUI/Demo Window" )]
internal class DemoWindow : BaseTab
{
	public override void Draw()
	{
		ImGui.ShowDemoWindow( ref visible );
	}
}
