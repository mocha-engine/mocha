﻿namespace Mocha.UI;

internal partial class UIManager
{
	public bool Debug { get; set; } = true;

	internal static UIManager Instance { get; private set; }

	private Menu MainMenu { get; }
	private Menu? SubMenu { get; set; }

	internal UIManager()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();

		MainMenu = new MainMenu();
	}

	public static void SetSubMenu( Menu? menu )
	{
		Instance.SubMenu?.Delete();
		Instance.SubMenu = menu;
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
			Graphics.DrawRectUnfilled( layout.Bounds, Theme.Green, 1f );
		}

		foreach ( var widget in widgets )
		{
			if ( widget.InputFlags.HasFlag( PanelInputFlags.MouseOver ) )
			{
				float thickness = widget.InputFlags.HasFlag( PanelInputFlags.MouseDown ) ? 4f : 1f;

				Graphics.DrawRectUnfilled( widget.Bounds, Theme.Red, thickness );

				var textBounds = new Rectangle( Input.MousePosition + 16, 512 );
				Graphics.DrawTextWithShadow( textBounds, $"{widget}:" );

				textBounds.Y += 20f;
				Graphics.DrawTextWithShadow( textBounds, $"{widget.Bounds}" );

				var parent = widget.Parent;
				while ( parent != null )
				{
					thickness = parent.InputFlags.HasFlag( PanelInputFlags.MouseDown ) ? 4f : 1f;

					Graphics.DrawRectUnfilled( parent.Bounds, Theme.Blue, thickness );
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