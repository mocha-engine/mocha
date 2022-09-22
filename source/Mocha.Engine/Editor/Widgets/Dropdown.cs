using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Dropdown : Button
{
	private List<Button> options = new();

	private bool drawOptions = false;
	private bool DrawOptions
	{
		get => drawOptions;
		set
		{
			drawOptions = value;
			options.ForEach( x => x.Visible = drawOptions );
		}
	}

	public string Selected
	{
		get => Text;
		private set => Text = value;
	}

	public int SelectedIndex { get; private set; }

	public Action<int> OnSelected;

	public Dropdown( string text ) : base( text )
	{
		OnClick = () =>
		{
			DrawOptions = !DrawOptions;
		};

		TextAnchor = new Vector2( 0f, 0.5f );
		ZIndex = 10;
		DrawOptions = false;
	}

	public void AddOption( string text )
	{
		int index = options.Count;

		var option = new Button( text, () =>
		{
			SelectedIndex = index;
			Selected = text;
			OnSelected?.Invoke( SelectedIndex );

			DrawOptions = false;
		} );

		option.Parent = this;
		option.Visible = false;

		options.Add( option );
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		base.Render( ref panelRenderer );

		var cursor = Bounds.Position + new Vector2( 0, 24 );

		foreach ( var option in options )
		{
			var desiredSize = option.GetDesiredSize();
			if ( desiredSize.X > Bounds.Width )
			{
				var newBounds = Bounds;
				newBounds.Width = desiredSize.X + 64f;
				Bounds = newBounds;

				Log.Trace( $"Dropdown entry was bigger than dropdown width, resizing dropdown to {desiredSize.X}" );
			}

			desiredSize.X = Bounds.Width;

			option.Bounds = new Rectangle( cursor, desiredSize );
			option.TextAnchor = new Vector2( 0f, 0.5f );
			cursor += new Vector2( 0, desiredSize.Y );
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return new Vector2( 128, 24 );
	}
}
