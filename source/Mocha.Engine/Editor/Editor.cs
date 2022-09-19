using Mocha.Renderer.UI;
using Veldrid;

namespace Mocha.Engine;

internal class Editor
{
	private PanelRenderer panelRenderer;

	internal Editor()
	{
		panelRenderer = new();
	}

	internal void Update()
	{
	}

	internal void Render( CommandList commandList )
	{
		panelRenderer.NewFrame();

		panelRenderer.AddRectangle( new Common.Rectangle( 16, 16, 512, 128 ),
			new Vector3( 0.15f, 0.15f, 0.15f ) );

		panelRenderer.AddRectangle( new Common.Rectangle( 16, 512, 128, 128 ),
			new Vector3( 0.15f, 0.15f, 0.5f ) );

		panelRenderer.Draw( commandList );
	}
}
