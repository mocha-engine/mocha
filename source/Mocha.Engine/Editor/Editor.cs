using Mocha.Renderer.UI;
using Veldrid;

namespace Mocha.Engine;

internal class Editor
{
	private PanelRenderer panelRenderer;
	private List<Panel> panels = new();

	internal Editor()
	{
		panelRenderer = new();

		panels.Add( new( new Common.Rectangle( 16, 16, 256, 64 ) ) );
	}

	internal void Render( CommandList commandList )
	{
		panelRenderer.NewFrame();
		panelRenderer.AddRectangle( new Common.Rectangle( 0f, (Vector2)Screen.Size ), new Vector3( 0.15f, 0.15f, 0.15f ) );

		for ( int i = 0; i < (4096 * Time.Delta).CeilToInt(); ++i )
		{
			float randX = (Random.Shared.NextSingle() * 2.0f) - 1.0f;
			randX *= 256f;

			float randY = (Random.Shared.NextSingle() * 2.0f) - 1.0f;
			randY *= 128f;

			var vel = new Vector2( randX, randY );
			var rect = new Common.Rectangle( Screen.Size.X / 2.0f, 32, 1, 1 );
			var color = new Vector3( Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle() );
			
			panels.Add( new PanelParticle( vel, color, rect ) );
		}

		foreach ( var panel in panels.ToArray() )
		{
			panel.Render( ref panelRenderer );

			if ( panel.rect.Y > Screen.Size.Y )
				panels.Remove( panel );
		}

		Log.Trace( $"{panels.Count} panels, {(1.0f / Time.Delta).CeilToInt()}fps" );

		panelRenderer.AddRectangle( new Common.Rectangle( Input.MousePosition.X, Input.MousePosition.Y, 4, 4 ), new Vector3( 0, 0, 0 ) );
		panelRenderer.Draw( commandList );
	}
}
