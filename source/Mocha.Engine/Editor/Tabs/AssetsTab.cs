using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.Folder} Assets/Browser" )]
internal class AssetsTab : BaseTab
{
	public static AssetsTab Instance { get; set; }

	private Texture ArchiveTexture { get; }
	private Texture DocumentTexture { get; }
	private Texture FolderTexture { get; }
	private Texture ImageTexture { get; }
	private Texture ModelTexture { get; }
	private Texture SoundTexture { get; }
	private Texture MaterialTexture { get; }

	private int selectedIndex;

	public List<(Texture, string)> fileSystemCache;

	private List<Texture> iconCache;

	private int maxIconsLoaded = 32;

	private int iconsLoadedThisFrame = 0;

	private float iconSize => 64;

	enum SortModes
	{
		DateAscending,
		DateDescending,
		Alphabetical
	};

	private SortModes sortMode = SortModes.DateAscending;

	public AssetsTab()
	{
		Instance = this;

		isVisible = true;

		ArchiveTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Archive.mtex" ).Build();
		DocumentTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Document.mtex" ).Build();
		FolderTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Folder.mtex" ).Build();
		ImageTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Image.mtex" ).Build();
		ModelTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Model.mtex" ).Build();
		SoundTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Sound.mtex" ).Build();
		MaterialTexture = TextureBuilder.UITexture.FromMochaTexture( "content/icons/Material.mtex" ).Build();

		fileSystemCache = new();
		iconCache = new();

		void CacheDirectory( string directory )
		{
			foreach ( var file in Directory.GetFiles( directory ) )
			{
				var icon = DocumentTexture;

				if ( file.EndsWith( "mtex" ) )
					icon = ImageTexture;
				else if ( file.EndsWith( "mmdl" ) )
					icon = ModelTexture;
				else if ( file.EndsWith( "mshdr" ) )
					icon = DocumentTexture;
				else if ( file.EndsWith( "mmat" ) )
					icon = MaterialTexture;

				fileSystemCache.Add( (icon, file) );
			}

			foreach ( var subDir in Directory.GetDirectories( directory ) )
			{
				CacheDirectory( subDir );
			}
		}

		CacheDirectory( "content/" );

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
		if ( name.EndsWith( "mtex" ) )
		{
			var texture = TextureBuilder.UITexture.FromMochaTexture( name ).Build();
			InspectorTab.SetSelectedObject( texture );
		}
		else if ( name.EndsWith( "mshdr" ) )
		{
			var shader = ShaderBuilder.Default.FromMoyaiShader( name ).Build();
			InspectorTab.SetSelectedObject( shader );
		}
		else if ( name.EndsWith( "mmdl" ) )
		{
			var model = Primitives.MochaModel.GenerateModels( name );
			InspectorTab.SetSelectedObject( model );
		}
		else if ( name.EndsWith( "mmat" ) )
		{
			var material = Material.FromMochaMaterial( name );
			InspectorTab.SetSelectedObject( material );
		}
	}

	string assetSearchText = "";

	public override void Draw()
	{
		ImGui.Begin( "Browser" );

		iconsLoadedThisFrame = 0;

		foreach ( var icon in iconCache.ToArray() )
		{
			if ( icon.VeldridTexture.IsDisposed )
				iconCache.Remove( icon );
		}

		{
			var sortString = sortMode switch
			{
				SortModes.DateAscending => $"{FontAwesome.ArrowUp} Date",
				SortModes.DateDescending => $"{FontAwesome.ArrowDown} Date",
				SortModes.Alphabetical => $"Name",
				_ => "Unsorted"
			};

			if ( ImGui.Button( sortString, new System.Numerics.Vector2( 128, 26 ) ) )
			{
				sortMode++;
				sortMode = (SortModes)((int)sortMode % 3);

				Sort();
			}

			ImGui.SameLine();
			ImGui.Button( $"{FontAwesome.File}" );
			ImGui.SameLine();
			ImGui.Button( $"{FontAwesome.WandSparkles}" );

			ImGui.SameLine();

			ImGui.SetNextItemWidth( -52 );
			ImGui.InputText( "##asset_search", ref assetSearchText, 128 );

			ImGui.SameLine();
			ImGui.Button( $"{FontAwesome.Gear}" );

			EditorHelpers.Separator();
		}

		{
			ImGui.BeginListBox( "##asset_list", new System.Numerics.Vector2( -1, -1 ) );

			var windowSize = ImGui.GetWindowSize();
			var windowPos = ImGui.GetWindowPos();

			Vector2 margin = new( iconSize / 4f );
			const int maxFileLength = 8;

			float startX = 16;
			var availableSpace = windowSize.X;
			availableSpace += margin.X / 2.0f;
			var remainingSpace = availableSpace % (iconSize + margin.X);
			startX = remainingSpace / 2.0f;

			float x = startX;
			float y = margin.Y;

			for ( int i = 0; i < fileSystemCache.Count; i++ )
			{
				var item = fileSystemCache[i];
				Texture icon = item.Item1;
				string name = item.Item2;

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


				var startPos = new System.Numerics.Vector2( x, y );

				if ( selectedIndex == i )
				{
					var drawList = ImGui.GetWindowDrawList();
					var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

					drawList.AddRectFilled(
						windowPos + startPos - new System.Numerics.Vector2( 4, 4 ) - scrollPos,
						windowPos + startPos + new System.Numerics.Vector2( iconSize + 4, iconSize + 20 ) - scrollPos,
						ImGui.GetColorU32( OneDark.Info * 0.75f ),
						4f );
				}

				ImGui.SetCursorPos( startPos );
				EditorHelpers.Image( icon, new Vector2( iconSize, iconSize ) );

				if ( name.EndsWith( ".mtex" ) )
				{
					if ( iconCache.Count < maxIconsLoaded && ImGui.IsItemVisible() && item.Item1 == ImageTexture && iconsLoadedThisFrame < 1 )
					{
						var loadedIcon = TextureBuilder.UITexture.FromMochaTexture( name ).Build();
						item.Item1 = loadedIcon;
						iconCache.Add( loadedIcon );

						fileSystemCache[i] = item;
						iconsLoadedThisFrame++;
					}

					if ( !ImGui.IsItemVisible() && selectedIndex != i )
					{
						if ( iconCache.Contains( item.Item1 ) )
						{
							item.Item1.Delete();
							item.Item1 = ImageTexture;
							fileSystemCache[i] = item;

							iconCache.Remove( item.Item1 );
						}
					}
				}

				ImGui.SetCursorPos( startPos );
				if ( ImGui.InvisibleButton( $"##{name}", new System.Numerics.Vector2( iconSize, iconSize + 24 ) ) )
				{
					selectedIndex = i;

					SelectItem( name );
				}

				var fileName = Path.GetFileName( name );
				var fileExtension = Path.GetExtension( name );
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension( name );

				var textSize = ImGui.CalcTextSize( fileName, iconSize );

				var textPos = (iconSize - textSize.X) / 2.0f;
				if ( textSize.Y > 16 )
					textPos = 0.0f;

				var textStartPos = startPos + new System.Numerics.Vector2( textPos, iconSize + 16 - textSize.Y );
				ImGui.SetCursorPos( textStartPos );

				void DrawShadowText( int x, int y )
				{
					ImGui.SetCursorPos( textStartPos );
					EditorHelpers.SetCursorPosXRelative( x );
					EditorHelpers.SetCursorPosYRelative( y );
					ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 0, 0, 0, 1 ) );
					ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + iconSize );
					ImGui.TextWrapped( fileName );
					ImGui.PopStyleColor();
				}

				DrawShadowText( 1, 1 );
				DrawShadowText( -1, -1 );
				DrawShadowText( 1, -1 );
				DrawShadowText( -1, -1 );

				ImGui.SetCursorPos( textStartPos );
				ImGui.PushTextWrapPos( ImGui.GetCursorPosX() + iconSize );
				ImGui.TextWrapped( fileName );

				ImGui.PopTextWrapPos();

				x += iconSize + margin.X;
				if ( x + iconSize + 16 > windowSize.X )
				{
					x = startX;
					y += iconSize + margin.Y + 24;
				}
			}

			ImGui.EndListBox();
		}

		ImGui.End();
	}
}
