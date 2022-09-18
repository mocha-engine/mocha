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
		panelRenderer.Draw( new EmptyUniformBuffer(), commandList );
	}
}
