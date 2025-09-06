﻿namespace Mocha.Editor;

[Icon( FontAwesome.Folder ), Title( "Browser" ), Category( "Game" )]
internal class BrowserWindow : EditorWindow
{
	public List<string> fileSystemCache = new();

	private int _selectedIndex;
	private string _assetSearchText = "";
	private Vector2 _iconSize;
	private List<ResourceType> _assetFilter = new();

	private Vector2 BaseIconSize => new( 100f, 150f );

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

		isVisible = false;
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

	private bool DrawIcon( float x, float y, string name, bool selected )
	{
		var fileExtension = Path.GetExtension( name );
		var fileName = Path.GetFileNameWithoutExtension( name );

		var resourceType = ResourceType.GetResourceForExtension( fileExtension ) ?? ResourceType.Default;

		var drawList = ImGui.GetWindowDrawList();
		var startPos = new System.Numerics.Vector2( x, y );

		var windowPos = ImGui.GetWindowPos();
		var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

		var icon = resourceType.IconLg;

		{
			drawList.AddRectFilledMultiColor(
				windowPos + startPos - new System.Numerics.Vector2( 8, 8 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( _iconSize.X + 8, _iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( resourceType.Color * 0.6f ),
				ImGui.GetColorU32( resourceType.Color * 0.6f ),
				ImGui.GetColorU32( resourceType.Color * 0.4f ),
				ImGui.GetColorU32( resourceType.Color * 0.4f )
			);

			drawList.AddRect(
				windowPos + startPos - new System.Numerics.Vector2( 10, 10 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( _iconSize.X + 10, _iconSize.Y + 10 ) - scrollPos,
				ImGui.GetColorU32( ImGuiCol.FrameBg ),
				4,
				ImDrawFlags.None,
				3 // rounding - 1px
			);

			drawList.AddRectFilled(
				windowPos + startPos + new System.Numerics.Vector2( -8, _iconSize.Y + 4 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( _iconSize.X + 8, _iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( resourceType.Color * 0.75f ),
				4f,
				ImDrawFlags.RoundCornersBottom );
		}

		Vector2 center = (_iconSize - 96f) / 2.0f;

		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( center.X, 24 + 2 ) );
		ImGuiX.Image( icon, new Vector2( 96f ), new System.Numerics.Vector4( 0, 0, 0, 0.1f ) );

		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( center.X, 24 ) );
		ImGuiX.Image( icon, new Vector2( 96f ) );

		if ( selected )
		{
			drawList.AddRect(
				windowPos + startPos - new System.Numerics.Vector2( 12, 12 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( _iconSize.X + 12, _iconSize.Y + 12 ) - scrollPos,
				ImGui.GetColorU32( Theme.Blue ),
				4f,
				ImDrawFlags.None,
				2f );
		}

		ImGui.SetCursorPos( startPos );
		if ( ImGui.InvisibleButton( $"##{name}", _iconSize ) )
		{
			return true;
		}

		var textSize = ImGui.CalcTextSize( fileName, _iconSize.X );

		var textPos = (_iconSize.X - textSize.X) / 2.0f;
		if ( textSize.Y > 16 )
			textPos = 0.0f;

		var textStartPos = startPos + new System.Numerics.Vector2( textPos, _iconSize.Y - textSize.Y - 4 );
		ImGui.SetCursorPos( textStartPos );

		ImGui.SetCursorPos( textStartPos );
		ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + _iconSize.X );
		ImGui.TextWrapped( fileName );
		ImGui.PopTextWrapPos();

		{
			ImGui.PushStyleColor( ImGuiCol.Text, resourceType.Color );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( 4, 0 ) );
			ImGui.Text( resourceType.IconSm );
			ImGui.PopStyleColor();

			float xOff = 16;

			ImGui.PushStyleColor( ImGuiCol.Text, Theme.Green );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( _iconSize.X - xOff, 0 ) );
			ImGui.Text( FontAwesome.Check );
			ImGui.PopStyleColor();

			if ( ImGui.IsItemHovered() )
				ImGui.SetTooltip( "Compiled & up-to-date" );

			xOff += 16;

			if ( name.Contains( "subaru" ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, Theme.Orange );
				ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( _iconSize.X - xOff, 0 ) );
				ImGui.Text( FontAwesome.Star );
				ImGui.PopStyleColor();
				xOff += 16;

				if ( ImGui.IsItemHovered() )
					ImGui.SetTooltip( "Favourited" );
			}

			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( _iconSize.X - xOff, 0 ) );
			ImGui.Text( FontAwesome.HardDrive );

			if ( ImGui.IsItemHovered() )
				ImGui.SetTooltip( "Local Asset" );
		}

		return false;
	}

	private void DrawAssetPopup()
	{
		ImGui.PushStyleVar( ImGuiStyleVar.PopupBorderSize, 2 );

		if ( ImGui.BeginPopup( "asset_popup", ImGuiWindowFlags.NoMove ) )
		{
			ImGui.PushStyleColor( ImGuiCol.Button, Theme.Gray );

			if ( ImGui.BeginTable( "##asset_list_buttons", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "a", ImGuiTableColumnFlags.WidthStretch, 1f );
				ImGui.TableSetupColumn( "b", ImGuiTableColumnFlags.WidthStretch, 1f );

				ImGui.TableNextColumn();

				if ( ImGui.Button( "All", new System.Numerics.Vector2( -1, 0 ) ) )
				{
					_assetFilter.Clear();
					_assetFilter.AddRange( ResourceType.All );
				}

				ImGui.TableNextColumn();

				if ( ImGui.Button( "None", new System.Numerics.Vector2( -1, 0 ) ) )
				{
					_assetFilter.Clear();
				}

				ImGui.EndTable();
			}

			if ( ImGui.BeginTable( "##asset_list_table", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "asset_toggles", ImGuiTableColumnFlags.WidthFixed, 0f );
				ImGui.TableSetupColumn( "asset_name", ImGuiTableColumnFlags.WidthStretch, 1f );
				ImGui.TableSetupColumn( "asset_solo", ImGuiTableColumnFlags.WidthFixed, 75f );

				foreach ( var resourceType in ResourceType.All )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					bool selected = _assetFilter.Contains( resourceType );
					if ( ImGui.Checkbox( $"##{resourceType.Name}_selected", ref selected ) )
					{
						if ( selected )
							_assetFilter.Add( resourceType );
						else
							_assetFilter.Remove( resourceType );
					}

					ImGui.TableNextColumn();
					ImGui.PushStyleColor( ImGuiCol.Text, resourceType.Color );
					ImGui.Text( $"{resourceType.IconSm.PadRight( 2 )} {resourceType.Name.PadRight( 32 )}" );
					ImGui.PopStyleColor();

					ImGui.TableNextColumn();

					if ( ImGui.Button( $"Solo##{resourceType.Name}_solo", new System.Numerics.Vector2( -1, 0 ) ) )
					{
						_assetFilter.Clear();
						_assetFilter.Add( resourceType );
					}
				}

				ImGui.EndTable();
			}

			ImGui.PopStyleColor();

			ImGui.EndPopup();
		}

		ImGui.PopStyleVar();
	}

	private void DrawFilterPopup()
	{
		ImGui.PushStyleVar( ImGuiStyleVar.PopupBorderSize, 2 );

		if ( ImGui.BeginPopup( "filter_popup", ImGuiWindowFlags.NoMove ) )
		{
			ImGui.PushStyleColor( ImGuiCol.Button, Theme.Gray );

			if ( ImGui.BeginTable( "##filter_list_buttons", 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "a", ImGuiTableColumnFlags.WidthStretch, 1f );
				ImGui.TableNextColumn();

				if ( ImGui.Button( "Clear", new System.Numerics.Vector2( -1, 0 ) ) )
				{
				}

				ImGui.EndTable();
			}

			if ( ImGui.BeginTable( "##filter_list_table", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "filter_toggles", ImGuiTableColumnFlags.WidthFixed, 0f );
				ImGui.TableSetupColumn( "filter_name", ImGuiTableColumnFlags.WidthStretch, 1f );

				var entries = new (System.Numerics.Vector4 Color, string Text)[]
				{
					( Theme.Orange, $"{FontAwesome.Star} {"Favourites",-32 }"),
					( Theme.Green, $"{FontAwesome.Check} {"Compiled Assets",-32 }"),
					( Vector4.One, $"{FontAwesome.HardDrive} {"Local Assets",-32 }"),
					( Vector4.One, $"{FontAwesome.Download} {"Downloaded Assets",-32 }"),
					( Vector4.One, $"{FontAwesome.Globe} {"Remote Assets",-32 }"),
				};

				foreach ( var entry in entries )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					bool selected = true;
					ImGui.Checkbox( $"##favourites_only_selected", ref selected );

					ImGui.TableNextColumn();
					ImGui.PushStyleColor( ImGuiCol.Text, entry.Color );
					ImGui.Text( entry.Text );
					ImGui.PopStyleColor();
				}

				ImGui.EndTable();
			}

			ImGui.PopStyleColor();

			ImGui.EndPopup();
		}

		ImGui.PopStyleVar();
	}

	public void DrawBrowser()
	{
		ImGui.PushStyleColor( ImGuiCol.ChildBg, Theme.Transparent );

		if ( ImGui.BeginChild( "sidebar", new System.Numerics.Vector2( 200, -1 ) ) )
		{
			var specialSources = new[]
			{
				$"{FontAwesome.ClockRotateLeft} Recent",
				$"{FontAwesome.Star} Favourites",
			};

			var localSources = new[]
			{
				$"{FontAwesome.FolderOpen} test project",
				$"{FontAwesome.MugHot} Mocha Core"
			};

			var onlineSources = new[]
			{
				$"{FontAwesome.Cubes} AmbientCG"
			};

			if ( ImGui.BeginListBox( "##sources", new System.Numerics.Vector2( -1, -1 ) ) )
			{
				ImGuiX.TextSubheading( $"{FontAwesome.FaceGrinStars} Special" );
				foreach ( var source in specialSources )
					ImGuiX.TextLight( source );

				ImGuiX.Separator();

				ImGuiX.TextSubheading( $"{FontAwesome.Folder} Local" );
				foreach ( var source in localSources )
					ImGuiX.TextLight( source );

				ImGuiX.Separator();

				ImGuiX.TextSubheading( $"{FontAwesome.Globe} Online" );
				foreach ( var source in onlineSources )
					ImGuiX.TextLight( source );

				ImGui.EndListBox();
			}

			ImGui.EndChild();
		}

		ImGui.SameLine();

		if ( ImGui.BeginChild( "main", new System.Numerics.Vector2( -1, -1 ) ) )
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

				ImGui.SetNextItemWidth( -243 );
				ImGui.InputText( "##asset_search", ref _assetSearchText, 128 );

				ImGui.SameLine();

				string suffix = (_assetFilter.Count > 0) ? FontAwesome.Asterisk : "";
				if ( ImGuiX.GradientButton( $"{FontAwesome.File} Asset{suffix}" ) )
				{
					ImGui.OpenPopup( "asset_popup" );
				}

				ImGui.SameLine();
				if ( ImGuiX.GradientButton( $"{FontAwesome.Filter} Filter" ) )
				{
					ImGui.OpenPopup( "filter_popup" );
				}

				ImGui.SameLine();
				if ( ImGuiX.GradientButton( $"{FontAwesome.Repeat}" ) )
				{
					CacheEverything();
				}

				ImGui.SameLine();
				ImGuiX.GradientButton( $"{FontAwesome.Gear}" );
			}

			ImGui.SetNextWindowPos( ImGui.GetWindowPos() + new System.Numerics.Vector2( ImGui.GetWindowWidth() - 380, 30 ) );
			DrawAssetPopup();

			ImGui.SetNextWindowPos( ImGui.GetWindowPos() + new System.Numerics.Vector2( ImGui.GetWindowWidth() - 290, 30 ) );
			DrawFilterPopup();

			if ( ImGui.BeginListBox( "##asset_list", new System.Numerics.Vector2( -1, -1 ) ) )
			{

				var windowSize = ImGui.GetWindowSize();
				var windowPos = ImGui.GetWindowPos();

				Vector2 margin = new( 24, 0 );

				float startPos = 16;

				var availableSpace = windowSize.X - startPos - 4f;
				var remainingSpace = availableSpace % (BaseIconSize.X + margin.X);

				int count = (int)windowSize.X / (int)(BaseIconSize.X + margin.X);

				_iconSize = BaseIconSize;
				_iconSize.X += (remainingSpace / count);

				float x = startPos;
				float y = startPos;

				for ( int i = 0; i < fileSystemCache.Count; i++ )
				{
					var name = fileSystemCache[i];

					if ( !string.IsNullOrEmpty( _assetSearchText ) )
					{
						bool foundAll = true;
						var inputs = _assetSearchText.Split( " " );

						foreach ( var input in inputs )
							if ( !name.Contains( input, StringComparison.CurrentCultureIgnoreCase ) )
								foundAll = false;

						if ( !foundAll )
							continue;
					}

					if ( _assetFilter.Count > 0 )
					{
						var resourceType = ResourceType.GetResourceForExtension( Path.GetExtension( name ) ) ?? ResourceType.Default;
						if ( !_assetFilter.Contains( resourceType ) )
							continue;
					}

					if ( DrawIcon( x, y, name, i == _selectedIndex ) )
					{
						SelectItem( name );
						_selectedIndex = i;
					}

					x += _iconSize.X + margin.X;
					if ( x + _iconSize.X > windowSize.X )
					{
						x = startPos;
						y += _iconSize.Y + margin.Y + 24;
					}

					ImGui.Dummy( new System.Numerics.Vector2( -1, _iconSize.Y ) );
				}

				ImGui.EndListBox();
			}

			ImGui.EndChild();
		}

		ImGui.PopStyleColor();

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
