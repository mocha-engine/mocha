using ImGuiNET;

namespace Mocha.Engine;

[Icon( FontAwesome.Folder ), Title( "Browser" ), Category( "Game" )]
internal class BrowserWindow : BaseEditorWindow
{
	public static BrowserWindow Instance { get; set; }

	private Texture ArchiveTexture { get; }
	private Texture DocumentTexture { get; }
	private Texture FolderTexture { get; }
	private Texture ImageTexture { get; }
	private Texture ModelTexture { get; }
	private Texture SoundTexture { get; }
	private Texture ShaderTexture { get; }
	private Texture MaterialTexture { get; }

	public List<(Texture, string)> fileSystemCache;
	private List<Texture> iconCache;

	private int maxIconsLoaded = 32;
	private int iconsLoadedThisFrame = 0;

	private int selectedIndex;
	private string assetSearchText = "";

	private Vector2 iconSize => new( 100f, 150f );

	enum SortModes
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

		ArchiveTexture = TextureBuilder.UITexture.FromPath( "icons/Archive.mtex" ).Build();
		DocumentTexture = TextureBuilder.UITexture.FromPath( "icons/Document.mtex" ).Build();
		FolderTexture = TextureBuilder.UITexture.FromPath( "icons/Folder.mtex" ).Build();
		ImageTexture = TextureBuilder.UITexture.FromPath( "icons/Image.mtex" ).Build();
		ModelTexture = TextureBuilder.UITexture.FromPath( "icons/Model.mtex" ).Build();
		SoundTexture = TextureBuilder.UITexture.FromPath( "icons/Sound.mtex" ).Build();
		MaterialTexture = TextureBuilder.UITexture.FromPath( "icons/Material.mtex" ).Build();
		ShaderTexture = TextureBuilder.UITexture.FromPath( "icons/Shader.mtex" ).Build();

		CacheEverything();
	}

	private void CacheEverything()
	{
		fileSystemCache = new();
		iconCache = new();

		void CacheDirectory( string directory )
		{
			foreach ( var fileName in FileSystem.Game.GetFiles( directory ) )
			{
				var sourceFileName = fileName;
				var isCompiled = false;
				var icon = DocumentTexture;

				if ( fileName.EndsWith( "_c" ) )
				{
					isCompiled = true;
					sourceFileName = fileName[..^2];
				}

				if ( sourceFileName.EndsWith( "mtex" ) )
				{
					icon = Texture.Builder.FromPath( fileName ).Build();
				}
				else if ( sourceFileName.EndsWith( "mmdl" ) )
					icon = ModelTexture;
				else if ( sourceFileName.EndsWith( "mshdr" ) )
					icon = ShaderTexture;
				else if ( sourceFileName.EndsWith( "mmat" ) )
				{
					icon = Material.FromPath( fileName ).DiffuseTexture;
				}

				var relativePath = Path.GetRelativePath( "content/", fileName );

				// Is this a compiled file with a source file present?
				if ( isCompiled && File.Exists( sourceFileName ) )
					continue;

				// Is this a mocha file?
				if ( !sourceFileName.Split( "." )[1].StartsWith( "m" ) )
					continue;

				fileSystemCache.Add( (icon, relativePath) );
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
				fileSystemCache = fileSystemCache.OrderBy( x => File.GetLastWriteTime( x.Item2 ) ).Reverse().ToList();
				break;
			case SortModes.DateDescending:
				fileSystemCache = fileSystemCache.OrderBy( x => File.GetLastWriteTime( x.Item2 ) ).ToList();
				break;
			case SortModes.Alphabetical:
				fileSystemCache.Sort( ( x, y ) => string.Compare( x.Item2, y.Item2 ) );
				break;
		}
	}

	public void SelectItem( string name )
	{
		if ( name.EndsWith( "_c", StringComparison.InvariantCultureIgnoreCase ) )
			name = name[..^2];

		if ( name.EndsWith( "mtex" ) )
		{
			var texture = TextureBuilder.UITexture.FromPath( name ).Build();
			InspectorWindow.SetSelectedObject( texture );
		}
		else if ( name.EndsWith( "mshdr" ) )
		{
			var shader = ShaderBuilder.Default.FromPath( name ).Build();
			InspectorWindow.SetSelectedObject( shader );
		}
		else if ( name.EndsWith( "mmdl" ) )
		{
			var model = Primitives.MochaModel.GenerateModels( name );
			InspectorWindow.SetSelectedObject( model );
		}
		else if ( name.EndsWith( "mmat" ) )
		{
			var material = Material.FromPath( name );
			InspectorWindow.SetSelectedObject( material );
		}
	}

	public System.Numerics.Vector4 GetColor( string name )
	{
		if ( name.EndsWith( "_c", StringComparison.InvariantCultureIgnoreCase ) )
			name = name[..^2];

		if ( name.EndsWith( "mtex" ) )
		{
			return MathX.GetColor( "#5292fa" );
		}
		else if ( name.EndsWith( "mshdr" ) )
		{
			return MathX.GetColor( "#ffc710" );
		}
		else if ( name.EndsWith( "mmdl" ) )
		{
			return MathX.GetColor( "#1ee3a5" );
		}
		else if ( name.EndsWith( "mmat" ) )
		{
			return MathX.GetColor( "#f7b239" );
		}

		return new System.Numerics.Vector4( 1, 0, 1, 1 );
	}

	private bool DrawIcon( float x, float y, Texture icon, string name, bool selected )
	{
		var drawList = ImGui.GetWindowDrawList();
		var startPos = new System.Numerics.Vector2( x, y );

		var windowPos = ImGui.GetWindowPos();
		var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

		{
			drawList.AddRectFilled(
				windowPos + startPos - new System.Numerics.Vector2( 8, 8 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 8, iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( GetColor( name ) * 0.4f ),
				4f );

			drawList.AddRectFilled(
				windowPos + startPos + new System.Numerics.Vector2( -8, iconSize.Y + 4 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 8, iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( GetColor( name ) * 0.75f ),
				4f,
				ImDrawFlags.RoundCornersBottom );
		}

		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( 2, 24 ) );
		ImGuiX.Image( icon, new Vector2( 96f, 96f ) );

		if ( selected )
		{
			drawList.AddRectFilled(
				windowPos + startPos - new System.Numerics.Vector2( 8, 8 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 8, iconSize.Y + 8 ) - scrollPos,
				ImGui.GetColorU32( Colors.Blue * 0.5f ),
				4f );

			drawList.AddRect(
				windowPos + startPos - new System.Numerics.Vector2( 10, 10 ) - scrollPos,
				windowPos + startPos + new System.Numerics.Vector2( iconSize.X + 10, iconSize.Y + 10 ) - scrollPos,
				ImGui.GetColorU32( Colors.Blue ),
				4f,
				ImDrawFlags.None,
				2f );
		}

		ImGui.SetCursorPos( startPos );
		if ( ImGui.InvisibleButton( $"##{name}", iconSize ) )
		{
			return true;
		}

		var fileName = Path.GetFileNameWithoutExtension( name );
		var textSize = ImGui.CalcTextSize( fileName, iconSize.X );

		var textPos = (iconSize.X - textSize.X) / 2.0f;
		if ( textSize.Y > 16 )
			textPos = 0.0f;

		var textStartPos = startPos + new System.Numerics.Vector2( textPos, iconSize.Y - textSize.Y - 4 );
		ImGui.SetCursorPos( textStartPos );

		void DrawShadowText( int x, int y )
		{
			ImGui.SetCursorPos( textStartPos );
			ImGuiX.SetCursorPosXRelative( x );
			ImGuiX.SetCursorPosYRelative( y );
			ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 0, 0, 0, 1 ) );
			ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + iconSize.X );
			ImGui.TextWrapped( fileName );
			ImGui.PopStyleColor();
		}

		DrawShadowText( 1, 1 );
		DrawShadowText( -1, 1 );
		DrawShadowText( 1, -1 );
		DrawShadowText( -1, -1 );

		ImGui.SetCursorPos( textStartPos );
		ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + iconSize.X );
		ImGui.TextWrapped( fileName );
		ImGui.PopTextWrapPos();

		{
			ImGui.PushStyleColor( ImGuiCol.Text, GetColor( name ) );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( -4, 0 ) );
			if ( name.EndsWith( "mmdl" ) )
				ImGui.Text( FontAwesome.Cube );
			else if ( name.EndsWith( "mtex" ) || name.EndsWith( "mtex_c" ) )
				ImGui.Text( FontAwesome.Image );
			else if ( name.EndsWith( "mshdr" ) )
				ImGui.Text( FontAwesome.Glasses );
			else if ( name.EndsWith( "mmat" ) )
				ImGui.Text( FontAwesome.Circle );
			ImGui.PopStyleColor();

			float xOff = 20;

			ImGui.PushStyleColor( ImGuiCol.Text, Colors.Green );
			ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( iconSize.X - xOff, 0 ) );
			ImGui.Text( FontAwesome.Check );
			ImGui.PopStyleColor();

			if ( ImGui.IsItemHovered() )
				ImGui.SetTooltip( "Compiled & up-to-date" );

			xOff += 16;

			if ( name.Contains( "subaru" ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, Colors.Orange );
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

	public override void Draw()
	{
		ImGui.Begin( "Browser" );

		iconsLoadedThisFrame = 0;

		foreach ( var icon in iconCache.ToArray() )
		{
			if ( icon.VeldridTexture.IsDisposed )
				iconCache.Remove( icon );
		}

		ImGui.PushStyleColor( ImGuiCol.ChildBg, Colors.Transparent );

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

				if ( ImGui.Button( $"{sortString}" ) )
				{
					sortMode++;
					sortMode = (SortModes)((int)sortMode % 3);

					Sort();
				}

				ImGui.SameLine();

				ImGui.SetNextItemWidth( -274 );
				ImGui.InputText( "##asset_search", ref assetSearchText, 128 );

				ImGui.SameLine();
				ImGui.Button( $"{FontAwesome.ChevronDown} Asset" );
				ImGui.SameLine();
				ImGui.Button( $"{FontAwesome.ChevronDown} Filter" );
				ImGui.SameLine();
				if ( ImGui.Button( $"{FontAwesome.Repeat}" ) )
				{
					CacheEverything();
				}

				ImGui.SameLine();
				ImGui.Button( $"{FontAwesome.Gear}" );
			}

			{
				ImGui.BeginListBox( "##asset_list", new System.Numerics.Vector2( -1, -1 ) );

				var windowSize = ImGui.GetWindowSize();
				var windowPos = ImGui.GetWindowPos();

				Vector2 margin = new( 24, 0 );

				float startPos = 16;

				var availableSpace = windowSize.X - startPos;
				var remainingSpace = availableSpace % (iconSize.X + margin.X);

				int count = (int)windowSize.X / (int)(iconSize.X + margin.X);
				margin.X += (remainingSpace / count);

				float x = startPos;
				float y = startPos;

				for ( int i = 0; i < fileSystemCache.Count; i++ )
				{
					var item = fileSystemCache[i];
					var icon = item.Item1;
					var name = item.Item2;

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

					if ( DrawIcon( x, y, icon, name, i == selectedIndex ) )
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

		ImGui.End();
	}
}
