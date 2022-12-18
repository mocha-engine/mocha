namespace Mocha.Engine.Editor;

internal partial class EditorInstance
{
	public bool Debug { get; set; } = false;

	internal static EditorInstance Instance { get; private set; }

	private List<Menu> Menus = new();

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();

		Menus.Add( new MainMenu() );
	}

	internal void Render()
	{
		UpdateWidgets();

		Graphics.PanelRenderer.NewFrame();
		RenderWidgets();
	}

	internal void RenderWidgets()
	{
		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.Render();
		}

		if ( !Debug )
			return;

		foreach ( var layout in BaseLayout.All )
		{
			Graphics.DrawRectUnfilled( layout.Bounds, Colors.Green, 1f );
		}

		foreach ( var widget in widgets )
		{
			if ( widget.InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
			{
				float thickness = widget.InputFlags.HasFlag( PanelInputFlags.MouseDown ) ? 4f : 1f;

				Graphics.DrawRectUnfilled( widget.Bounds, Colors.Red, thickness );

				var textBounds = new Rectangle( Input.MousePosition + 16, 256 );
				Graphics.DrawTextWithShadow( textBounds, $"{widget}:" );

				textBounds.Y += 20f;
				Graphics.DrawTextWithShadow( textBounds, $"{widget.Bounds}" );

				var parent = widget.Parent;
				while ( parent != null )
				{
					thickness = parent.InputFlags.HasFlag( PanelInputFlags.MouseDown ) ? 4f : 1f;

					Graphics.DrawRectUnfilled( parent.Bounds, Colors.Blue, thickness );
					parent = parent.Parent;
				}
			}
		}
	}

	internal void UpdateWidgets()
	{
		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.InputFlags = PanelInputFlags.None;
		}

		if ( mouseOverWidgets.Any() )
		{
			var focusedWidget = mouseOverWidgets.Last();
			focusedWidget.InputFlags |= PanelInputFlags.MouseOver;

			if ( Input.Left )
			{
				focusedWidget.InputFlags |= PanelInputFlags.MouseDown;
			}
		}

		foreach ( var widget in widgets )
		{
			widget.Update();
		}
	}
}
