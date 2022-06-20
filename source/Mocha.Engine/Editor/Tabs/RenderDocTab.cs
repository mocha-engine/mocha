using ImGuiNET;
using Veldrid;

namespace Mocha.Engine;

[EditorMenu( "Debug/RenderDoc" )]
internal class RenderDocTab : BaseTab
{
	private RenderDoc renderDoc;

	public RenderDocTab()
	{

	}

	public override void Draw()
	{
		ImGui.Begin( "RenderDoc", ref visible );

		bool loaded = renderDoc != null;

		ImGui.Text( $"RenderDoc is{(loaded ? "" : " not")} loaded." );

		if ( ImGui.Button( "Load" ) )
		{
			if ( Veldrid.RenderDoc.Load( out renderDoc ) )
			{
				renderDoc.OverlayEnabled = false;
				Log.Trace( "Loaded RenderDoc" );
			}
		}

		ImGui.End();
	}
}
