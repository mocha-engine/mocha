namespace Mocha.Engine.Editor;

internal class Dropdown : Button
{
	private List<Selectable> options = new();
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
		DrawOptions = false;
	}

	public void AddOption( string text )
	{
		int index = options.Count;

		var option = new Selectable( text, () =>
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

	internal override void Render()
	{
		base.Render();

		var cursor = Bounds.Position + new Vector2( 0, GetDesiredSize().Y );
		var iconBounds = new Rectangle( Bounds.X + Bounds.Width - 26, Bounds.Y + ((Bounds.Height - 16) / 2.0f), 16, 16 );
		Graphics.DrawText( iconBounds, FontAwesome.CircleChevronDown );

		var iconBackgroundBounds = new Rectangle( iconBounds.X - 8, iconBounds.Y, 0, 0 ).Expand( 6 );
		iconBackgroundBounds.Size = new( 40, 28 );
		Graphics.DrawRect( iconBackgroundBounds, ITheme.Current.ButtonBgB * 0.1f, RoundingFlags.Right );

		iconBackgroundBounds.Height = Bounds.Height;
		iconBackgroundBounds.Y = Bounds.Y;
		iconBackgroundBounds = iconBackgroundBounds.Shrink( 1f );
		iconBackgroundBounds.X -= 1;
		iconBackgroundBounds.Width = 1f;
		Graphics.DrawRect( iconBackgroundBounds, ITheme.Current.Border );
		iconBackgroundBounds.X -= 1;
		Graphics.DrawRect( iconBackgroundBounds, ITheme.Current.ButtonBgA );

		foreach ( Selectable? option in options )
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

		ZIndex = 0;

		if ( DrawOptions )
		{
			Vector4 colorA = ITheme.Current.ButtonBgA;
			Vector4 colorB = ITheme.Current.ButtonBgB;
			Vector4 border = ITheme.Current.Border;

			var optionBounds = Bounds;
			optionBounds.Position += new Vector2( 0, GetDesiredSize().Y - 1 );
			optionBounds.Size = optionBounds.Size.WithY( GetDesiredSize().Y * options.Count );

			Graphics.DrawShadow( optionBounds, 4f, ITheme.Current.ShadowOpacity );
			Graphics.DrawRect( optionBounds, border, RoundingFlags.Bottom );
			Graphics.DrawRect( optionBounds.Shrink( 1f ),
					colorA,
					colorB,
					colorA,
					colorB,
					RoundingFlags.Bottom
			);

			for ( int i = 0; i < options.Count; i++ )
			{
				var option = options[i];
				if ( i != options.Count && i != 0 )
				{
					var b = Bounds.Shrink( 10f );
					b.Y = option.Bounds.Y;
					b.Height = 1f;
					Graphics.DrawRect( b, ITheme.Current.ButtonBgB, RoundingFlags.All );
					b.Y += 1f;
					Graphics.DrawRect( b, ITheme.Current.ButtonBgA, RoundingFlags.All );
				}
			}

			ZIndex = 10;
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		var baseSize = base.GetDesiredSize();
		baseSize.X = 256;
		return baseSize;
	}

	internal override void OnDelete()
	{
		base.OnDelete();

		options.ForEach( x => x.Delete() );
	}
}
