﻿namespace Mocha.Editor;

[Icon( FontAwesome.Folder ), Title( "Browser" ), Category( "Game" )]
internal class BrowserWindow : EditorWindow
{
	public static BrowserWindow Instance { get; set; }
	public List<string> fileSystemCache = new();

	private int selectedIndex;
	private string assetSearchText = "";

	private Vector2 baseIconSize => new( 100f, 150f );
	private Vector2 iconSize;

	private List<FileType> assetFilter = new();

	private enum SortModes
	{
		DateAscending,
		DateDescending,
		Alphabetical
	};

	private SortModes sortMode = SortModes.DateAscending;

	public BrowserWindow()
	{
		Instance = this;
		isVisible = true;

		CacheEverything();
	}

	private void CacheEverything()
	{
		fileSystemCache.Clear();

		void CacheDirectory( string directory )
		{
			foreach ( var fileName in FileSystem.Game.GetFiles( directory ) )
			{
				var sourceFileName = fileName;
				var isCompiled = false;

				if ( fileName.EndsWith( "_c" ) )
				{
					isCompiled = true;
					sourceFileName = fileName[..^2];
				}

				var relativePath = FileSystem.Game.GetRelativePath( fileName );

				// Is this a compiled file with a source file present?
				if ( isCompiled && File.Exists( sourceFileName ) )
					continue;

				// Is this a mocha file?
				if ( !sourceFileName.Split( "." )[1].StartsWith( "m" ) )
					continue;

				fileSystemCache.Add( relativePath );
			}

			foreach ( var subDir in FileSystem.Game.GetDirectories( directory ) )
			{
				CacheDirectory( subDir );
			}
		}

		CacheDirectory( "" );

		Sort();
	}

	private void Sort()
	{
		switch ( sortMode )
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

	public void SelectItem( string name )
	{
		// TODO
	}

	private bool DrawIcon( float x, float y, string name, bool selected )
	{
		var fileExtension = Path.GetExtension( name );
		var fileName = Path.GetFileNameWithoutExtension( name );

		var fileType = FileType.GetFileTypeForExtension( fileExtension ) ?? FileType.Default;

		var drawList = ImGui.GetWindowDrawList();
		var startPos = new System.Numerics.Vector2( x, y );

		var windowPos = ImGui.GetWindowPos();
		var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

		var icon = Texture.FromCache( fileType.IconLg );

		{
			drawList.AddRectFilledMultiColor(
				windowPos + startPos - new System.Numerics.Vector2( 8, 8 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 8, iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( fileType.Color * 0.6f ),
				ImGui.GetColorU32( fileType.Color * 0.6f ),
				ImGui.GetColorU32( fileType.Color * 0.4f ),
				ImGui.GetColorU32( fileType.Color * 0.4f )
			);

			drawList.AddRect(
				windowPos + startPos - new System.Numerics.Vector2( 10, 10 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 10, iconSize.Y + 10 ) - scrollPos,
				ImGui.GetColorU32( ImGuiCol.FrameBg ),
				4,
				ImDrawFlags.None,
				3 // rounding - 1px
			);

			drawList.AddRectFilled(
				windowPos + startPos + new System.Numerics.Vector2( -8, iconSize.Y + 4 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 8, iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( fileType.Color * 0.75f ),
				4f,
				ImDrawFlags.RoundCornersBottom );
		}

		Vector2 center = (iconSize - 96f) / 2.0f;

		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( center.X, 24 + 2 ) );
		ImGuiX.Image( icon, new Vector2( 96f ), new System.Numerics.Vector4( 0, 0, 0, 0.1f ) );

		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( center.X, 24 ) );
		ImGuiX.Image( icon, new Vector2( 96f ) );

		if ( selected )
		{
			drawList.AddRect(
				windowPos + startPos - new System.Numerics.Vector2( 12, 12 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 12, iconSize.Y + 12 ) - scrollPos,
				ImGui.GetColorU32( Theme.Blue ),
				4f,
				ImDrawFlags.None,
				2f );
		}

		ImGui.SetCursorPos( startPos );
		if ( ImGui.InvisibleButton( $"##{name}", iconSize ) )
		{
			return true;
		}

		var textSize = ImGui.CalcTextSize( fileName, iconSize.X );

		var textPos = (iconSize.X - textSize.X) / 2.0f;
		if ( textSize.Y > 16 )
			textPos = 0.0f;

		var textStartPos = startPos + new System.Numerics.Vector2( textPos, iconSize.Y - textSize.Y - 4 );
		ImGui.SetCursorPos( textStartPos );

		ImGui.SetCursorPos( textStartPos );
		ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + iconSize.X );
		ImGui.TextWrapped( fileName );
		ImGui.PopTextWrapPos();

		{
			ImGui.PushStyleColor( ImGuiCol.Text, fileType.Color );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( 4, 0 ) );
			ImGui.Text( fileType.IconSm );
			ImGui.PopStyleColor();

			float xOff = 16;

			ImGui.PushStyleColor( ImGuiCol.Text, Theme.Green );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( iconSize.X - xOff, 0 ) );
			ImGui.Text( FontAwesome.Check );
			ImGui.PopStyleColor();

			if ( ImGui.IsItemHovered() )
				ImGui.SetTooltip( "Compiled & up-to-date" );

			xOff += 16;

			if ( name.Contains( "subaru" ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, Theme.Orange );
				ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( iconSize.X - xOff, 0 ) );
				ImGui.Text( FontAwesome.Star );
				ImGui.PopStyleColor();
				xOff += 16;

				if ( ImGui.IsItemHovered() )
					ImGui.SetTooltip( "Favourited" );
			}

			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( iconSize.X - xOff, 0 ) );
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
					assetFilter.Clear();
					assetFilter.AddRange( FileType.All );
				}

				ImGui.TableNextColumn();

				if ( ImGui.Button( "None", new System.Numerics.Vector2( -1, 0 ) ) )
				{
					assetFilter.Clear();
				}

				ImGui.EndTable();
			}

			if ( ImGui.BeginTable( "##asset_list_table", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "asset_toggles", ImGuiTableColumnFlags.WidthFixed, 0f );
				ImGui.TableSetupColumn( "asset_name", ImGuiTableColumnFlags.WidthStretch, 1f );
				ImGui.TableSetupColumn( "asset_solo", ImGuiTableColumnFlags.WidthFixed, 75f );

				foreach ( var fileType in FileType.All )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					bool selected = assetFilter.Contains( fileType );
					if ( ImGui.Checkbox( $"##{fileType.Name}_selected", ref selected ) )
					{
						if ( selected )
							assetFilter.Add( fileType );
						else
							assetFilter.Remove( fileType );
					}

					ImGui.TableNextColumn();
					ImGui.PushStyleColor( ImGuiCol.Text, fileType.Color );
					ImGui.Text( $"{fileType.IconSm.PadRight( 2 )} {fileType.Name.PadRight( 32 )}" );
					ImGui.PopStyleColor();

					ImGui.TableNextColumn();

					if ( ImGui.Button( $"Solo##{fileType.Name}_solo", new System.Numerics.Vector2( -1, 0 ) ) )
					{
						assetFilter.Clear();
						assetFilter.Add( fileType );
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

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "Browser", ref isVisible ) )
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

				ImGui.BeginListBox( "##sources", new System.Numerics.Vector2( -1, -1 ) );

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
				ImGui.EndChild();
			}

			ImGui.SameLine();

			if ( ImGui.BeginChild( "main", new System.Numerics.Vector2( -1, -1 ) ) )
			{
				{
					var sortString = sortMode switch
					{
						SortModes.DateAscending => $"{FontAwesome.CalendarPlus}",
						SortModes.DateDescending => $"{FontAwesome.CalendarMinus}",
						SortModes.Alphabetical => $"{FontAwesome.ArrowDownAZ}",
						_ => "Unsorted"
					};

					if ( ImGuiX.GradientButton( $"{sortString}" ) )
					{
						sortMode++;
						sortMode = (SortModes)((int)sortMode % 3);

						Sort();
					}

					ImGui.SameLine();

					ImGui.SetNextItemWidth( -243 );
					ImGui.InputText( "##asset_search", ref assetSearchText, 128 );

					ImGui.SameLine();

					string suffix = (assetFilter.Count > 0) ? FontAwesome.Asterisk : "";
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
					var remainingSpace = availableSpace % (baseIconSize.X + margin.X);

					int count = (int)windowSize.X / (int)(baseIconSize.X + margin.X);

					iconSize = baseIconSize;
					iconSize.X += (remainingSpace / count);

					float x = startPos;
					float y = startPos;

					for ( int i = 0; i < fileSystemCache.Count; i++ )
					{
						var name = fileSystemCache[i];

						if ( !string.IsNullOrEmpty( assetSearchText ) )
						{
							bool foundAll = true;
							var inputs = assetSearchText.Split( " " );

							foreach ( var input in inputs )
								if ( !name.Contains( input, StringComparison.CurrentCultureIgnoreCase ) )
									foundAll = false;

							if ( !foundAll )
								continue;
						}

						if ( assetFilter.Count > 0 )
						{
							var fileType = FileType.GetFileTypeForExtension( Path.GetExtension( name ) ) ?? FileType.Default;
							if ( !assetFilter.Contains( fileType ) )
								continue;
						}

						if ( DrawIcon( x, y, name, i == selectedIndex ) )
						{
							SelectItem( name );
							selectedIndex = i;
						}

						x += iconSize.X + margin.X;
						if ( x + iconSize.X > windowSize.X )
						{
							x = startPos;
							y += iconSize.Y + margin.Y + 24;
						}

						ImGui.Dummy( new System.Numerics.Vector2( -1, iconSize.Y ) );
					}

					ImGui.EndListBox();
				}

				ImGui.EndChild();
			}

			ImGui.PopStyleColor();

		}

		ImGui.End();
	}
}
