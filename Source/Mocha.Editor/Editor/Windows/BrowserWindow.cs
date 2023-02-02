namespace Mocha.Editor;

[Icon( FontAwesome.Folder ), Title( "Browser" ), Category( "Game" )]
internal class BrowserWindow : EditorWindow
{
	public List<string> fileSystemCache = new();

	private int _selectedIndex;
	private string _assetSearchText = "";

	private enum SortModes
	{
		DateAscending,
		DateDescending,
		Alphabetical
	};

	private SortModes _sortMode = SortModes.DateAscending;

	public BrowserWindow()
	{
		CacheEverything();
	}

	public void SelectItem( string name )
	{
		if ( name.EndsWith( "_c", StringComparison.InvariantCultureIgnoreCase ) )
			name = name[..^2];

		if ( name.EndsWith( "mtex" ) )
		{
			var texture = new Texture( name );
			InspectorWindow.SetSelectedObject( texture );
		}
		else if ( name.EndsWith( "mmdl" ) )
		{
			var model = new Model( name );
			InspectorWindow.SetSelectedObject( model );
		}
		else if ( name.EndsWith( "mmat" ) )
		{
			var material = new Material( name );
			InspectorWindow.SetSelectedObject( material );
		}
	}

	private void CacheEverything()
	{
		fileSystemCache.Clear();

		void CacheDirectory( string directory )
		{
			foreach ( var fileName in FileSystem.Mounted.GetFiles( directory ) )
			{
				var sourceFileName = fileName;
				var isCompiled = false;

				if ( fileName.EndsWith( "_c" ) )
				{
					isCompiled = true;
					sourceFileName = fileName[..^2];
				}

				var relativePath = FileSystem.Mounted.GetRelativePath( fileName );

				// Is this a compiled file with a source file present?
				if ( isCompiled && File.Exists( sourceFileName ) )
					continue;

				// Is this a mocha file?
				if ( !sourceFileName.Split( "." )[1].StartsWith( "m" ) )
					continue;

				fileSystemCache.Add( relativePath );
			}

			foreach ( var subDir in FileSystem.Mounted.GetDirectories( directory ) )
			{
				CacheDirectory( subDir );
			}
		}

		CacheDirectory( "." );

		Sort();
	}

	private void Sort()
	{
		switch ( _sortMode )
		{
			case SortModes.DateAscending:
				fileSystemCache = fileSystemCache.OrderBy( x => File.GetLastWriteTime( x ) ).Reverse().ToList();
				break;
			case SortModes.DateDescending:
				fileSystemCache = fileSystemCache.OrderBy( x => File.GetLastWriteTime( x ) ).ToList();
				break;
			case SortModes.Alphabetical:
				fileSystemCache.Sort( ( x, y ) => string.Compare( x, y ) );
				break;
		}
	}

	private void DrawAsset( string name, int index )
	{
		var fileExtension = Path.GetExtension( name );
		var fileName = Path.GetFileNameWithoutExtension( name );

		var resourceType = ResourceType.GetResourceForExtension( fileExtension ) ?? ResourceType.Default;

		float w = ImGui.GetWindowSize().X - 100;

		if ( ImGui.GetCursorPos().X > w )
		{
			ImGui.NewLine();
			ImGuiX.BumpCursorX( 8 );
		}

		if ( ImGuiX.Icon( fileName, resourceType.Name, resourceType.IconLg, resourceType.Color, index == _selectedIndex ) )
		{
			SelectItem( name );
			_selectedIndex = index;
		}

		ImGui.SameLine();
	}

	public void DrawBrowser()
	{
		{
			var sortString = _sortMode switch
			{
				SortModes.DateAscending => $"{FontAwesome.CalendarPlus}",
				SortModes.DateDescending => $"{FontAwesome.CalendarMinus}",
				SortModes.Alphabetical => $"{FontAwesome.ArrowDownAZ}",
				_ => "Unsorted"
			};

			if ( ImGuiX.GradientButton( $"{sortString}" ) )
			{
				_sortMode++;
				_sortMode = (SortModes)((int)_sortMode % 3);

				Sort();
			}

			ImGui.SameLine();

			ImGui.SetNextItemWidth( -46 );
			ImGui.InputText( "##asset_search", ref _assetSearchText, 128 );

			ImGui.SameLine();
			if ( ImGuiX.GradientButton( $"{FontAwesome.Repeat}" ) )
			{
				CacheEverything();
			}
		}

		if ( ImGui.BeginListBox( "##asset_list", new System.Numerics.Vector2( -1, -1 ) ) )
		{
			float padding = 8f;
			ImGuiX.BumpCursorX( padding );
			ImGuiX.BumpCursorY( padding );

			for ( int i = 0; i < fileSystemCache.Count; i++ )
			{
				var name = fileSystemCache[i];

				if ( !string.IsNullOrEmpty( _assetSearchText ) )
				{
					bool foundAll = true;
					var inputs = _assetSearchText.Split( " " );

					foreach ( var input in inputs )
					{
						if ( !name.Contains( input, StringComparison.CurrentCultureIgnoreCase ) )
							foundAll = false;
					}

					if ( !foundAll )
						continue;
				}

				DrawAsset( name, i );
			}

			ImGui.EndListBox();
		}

	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "Browser", ref isVisible ) )
		{
			DrawBrowser();

			ImGui.End();
		}
	}
}
