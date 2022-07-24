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

	private int selectedIndex;

	public List<(Texture, string)> fileSystemCache;

	private List<Texture> iconCache;

	private int maxIconsLoaded = 32;

	private int iconsLoadedThisFrame = 0;

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
			ImGui.BeginChild( "##asset_list" );

			var windowSize = ImGui.GetWindowSize();
			var windowPos = ImGui.GetWindowPos();

			Vector2 margin = new( 32, 8 );
			const float size = 64;
			const int maxFileLength = 8;

			float x = 16;
			float y = margin.Y;

			for ( int i = 0; i < fileSystemCache.Count; i++ )
			{
				var item = fileSystemCache[i];
				Texture icon = item.Item1;
				string name = item.Item2;

				var startPos = new System.Numerics.Vector2( x, y );

				if ( selectedIndex == i )
				{
					var drawList = ImGui.GetWindowDrawList();
					var scrollPos = new System.Numerics.Vector2( 0, ImGui.GetScrollY() );

					drawList.AddRectFilled(
						windowPos + startPos - new System.Numerics.Vector2( 4, 4 ) - scrollPos,
						windowPos + startPos + new System.Numerics.Vector2( size + 4, size + 4 ) - scrollPos,
						ImGui.GetColorU32( OneDark.Info * 0.75f ),
						4f );
				}

				ImGui.SetCursorPos( startPos );
				EditorHelpers.Image( icon, new Vector2( size, size ) );

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
				if ( ImGui.InvisibleButton( $"##{name}", new System.Numerics.Vector2( size, size + 24 ) ) )
				{
					selectedIndex = i;

					SelectItem( name );
				}

				var fileName = Path.GetFileName( name );
				var fileExtension = Path.GetExtension( name );
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension( name );

				if ( fileNameWithoutExtension.Length > maxFileLength )
					fileName = fileName[..maxFileLength] + ".." + fileExtension;

				var textPos = (size - ImGui.CalcTextSize( fileName ).X) / 2.0f;
				ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( textPos, size + 4f ) );

				ImGui.Text( fileName );

				x += size + margin.X;
				if ( x + size + 16 > windowSize.X )
				{
					x = 16;
					y += size + margin.Y + 24;
				}
			}

			ImGui.EndChild();
		}

		ImGui.End();
	}
}
