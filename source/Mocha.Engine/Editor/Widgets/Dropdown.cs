using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Dropdown : Widget
{
	private Button button;

	private List<Button> options = new();
	private bool drawOptions = false;

	public string Selected
	{
		get => button.Text;
		private set => button.Text = value;
	}

	public int SelectedIndex { get; private set; }

	public Action<int> OnSelected;

	public Dropdown( string text )
	{
		button = new( text, () =>
		{
			drawOptions = !drawOptions;
		} );

		drawOptions = new();

		ZIndex = 10;
	}

	public void AddOption( string text )
	{
		int index = options.Count;

		var option = new Button( text, () =>
		{
			SelectedIndex = index;
			Selected = text;
			OnSelected?.Invoke( SelectedIndex );

			drawOptions = false;
		} );

		options.Add( option );
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		button.Bounds = Bounds;
		button.TextAnchor = new Vector2( 0f, 0.5f );
		button.Render( ref panelRenderer );

		if ( !drawOptions )
			return;


		var cursor = Bounds.Position + new Vector2( 0, 24 );
		panelRenderer.AddRectangle( new Rectangle( cursor, new Vector2( Bounds.Width, options.Count * 24 ) ), Colors.Blue );

		foreach ( var option in options )
		{
			var desiredSize = option.GetDesiredSize();
			desiredSize.X = Bounds.Width;

			option.Bounds = new Rectangle( cursor, desiredSize );
			option.TextAnchor = new Vector2( 0f, 0.5f );
			option.Render( ref panelRenderer );

			cursor += new Vector2( 0, desiredSize.Y );
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return new Vector2( 128, 24 );
	}
}
