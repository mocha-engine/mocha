using System.Reflection;

namespace Mocha.Editor;

public class Editor
{
	private List<BaseEditorWindow> windows = new();

	private void DrawMenuBar()
	{
		EditorUI.BeginMainMenuBar();

		if ( EditorUI.BeginMenu( $"Tools" ) )
		{
			EditorUI.MenuItem( FontAwesome.Image, "Texture Tool" );
			EditorUI.MenuItem( FontAwesome.FaceGrinStars, "Material Tool" );
			EditorUI.MenuItem( FontAwesome.Cubes, "Model Tool" );
			EditorUI.MenuItem( FontAwesome.Glasses, "Shader Tool" );
			EditorUI.EndMenu();
		}

		foreach ( var window in windows )
		{
			var displayInfo = DisplayInfo.For( window );

			if ( EditorUI.BeginMenu( displayInfo.Category ) )
			{
				var enabled = window.isVisible;
				bool active = EditorUI.MenuItemEx( displayInfo.TextIcon, displayInfo.Name, enabled );

				if ( active )
					window.isVisible = !window.isVisible;

				EditorUI.EndMenu();
			}
		}

		EditorUI.EndMainMenuBar();
	}

	public Editor()
	{
		windows.AddRange( Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where( x => typeof( BaseEditorWindow ).IsAssignableFrom( x ) )
			.Where( x => x != typeof( BaseEditorWindow ) )
			.Select( x => Activator.CreateInstance( x ) )
			.OfType<BaseEditorWindow>()
		);
	}

	public void Render()
	{
		DrawMenuBar();

		EditorUI.ShowDemoWindow();

		foreach ( var window in windows )
		{
			if ( window.isVisible )
				window.Render();
		}
	}
}

