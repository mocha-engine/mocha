//using System.Reflection;
//using System.Text.Json;

//namespace Mocha.Editor;

//[Title( "Material Editor" )]
//public class MaterialEditorWindow : EditorWindow
//{
//	private List<(string Path, MaterialInfo Material)> LoadedMaterials { get; } = new();
//	private int CurrentMaterialIndex { get; set; } = -1;

//	private MaterialInfo? CurrentMaterial
//	{
//		get
//		{
//			if ( CurrentMaterialIndex < 0 || CurrentMaterialIndex >= LoadedMaterials.Count )
//				return null;

//			return LoadedMaterials[CurrentMaterialIndex].Material;
//		}
//	}

//	private string CurrentPath
//	{
//		get
//		{
//			if ( CurrentMaterialIndex < 0 || CurrentMaterialIndex >= LoadedMaterials.Count )
//				return "unnamed.mmat";

//			return LoadedMaterials[CurrentMaterialIndex].Path;
//		}
//	}

//	private Dictionary<string, Texture> TextureCache = new();

//	private void LoadMaterial( string path )
//	{
//		// Check if already loaded
//		if ( !LoadedMaterials.Any( x => x.Path == path ) )
//		{
//			// Load material
//			var data = File.ReadAllText( FileSystem.Game.GetAbsolutePath( path, true, true ) );
//			var loadedMaterial = JsonSerializer.Deserialize<MaterialInfo>( data );

//			LoadedMaterials.Add( (path, loadedMaterial) );
//		}

//		// Set it as current
//		CurrentMaterialIndex = LoadedMaterials.FindIndex( x => x.Path == path );
//	}

//	private void NewMaterial()
//	{
//		CurrentMaterialIndex = LoadedMaterials.Count;
//		LoadedMaterials.Add( ("unnamed.mmat", new()) );
//	}

//	private void DrawMaterialTexture( PropertyInfo propertyInfo )
//	{
//		var name = propertyInfo.Name.DisplayName();
//		var currentPath = propertyInfo.GetValue( CurrentMaterial ) as string;
//		var y = ImGui.GetCursorY();
//		var x = ImGui.GetCursorX();

//		ImGui.Text( name );
//		ImGui.Text( currentPath ?? "None" );

//		if ( ImGui.Button( $"{FontAwesome.FolderOpen} Replace...##{propertyInfo.GetHashCode()}" ) )
//		{
//			var openFileDialog = new OpenFileWindow();
//			openFileDialog.Filter = "*.png, *.jpg";
//			openFileDialog.OnSelected += ( path ) =>
//			{
//				path = Path.ChangeExtension( path, "mtex" );

//				var loadedMat = LoadedMaterials[CurrentMaterialIndex];

//				// SetValue does not work on non-boxed struct
//				object boxed = loadedMat.Material;
//				propertyInfo.SetValue( boxed, path );
//				loadedMat.Material = (MaterialInfo)boxed;

//				LoadedMaterials[CurrentMaterialIndex] = loadedMat;
//			};

//			openFileDialog.Show();
//		}

//		var width = ImGui.GetColumnWidth();
//		ImGui.SetCursorY( y );
//		ImGui.BumpCursorX( width - 64 );

//		if ( currentPath != null && FileSystem.Game.Exists( currentPath ) )
//		{
//			ImGui.Image( GetTexture( currentPath ).NativeTexture, 64, 64 );
//		}

//		ImGui.SetCursorPos( x, y + 64f );
//		ImGui.BumpCursorY( 8f );
//	}

//	private void DrawMaterialProperties()
//	{
//		foreach ( var property in typeof( MaterialInfo ).GetProperties() )
//		{
//			DrawMaterialTexture( property );

//			ImGui.SeparatorH();
//		}
//	}

//	private void DrawToolBar()
//	{
//		var items = new List<(string text, Action onClick)>
//		{
//			( $"{FontAwesome.FileCirclePlus} Create...",
//				NewMaterial
//			),
//			( $"{FontAwesome.FolderOpen} Browse...",
//				() => {
//					var openFileDialog = new OpenFileWindow();
//					openFileDialog.Filter = "*.mmat";
//					openFileDialog.OnSelected += ( path ) =>
//					{
//						LoadMaterial( path );
//					};

//					openFileDialog.Show();
//				}
//			),
//			( "separator", null ),
//			( $"{FontAwesome.FloppyDisk} Save",
//				() => {
//					var data = JsonSerializer.Serialize<MaterialInfo>( CurrentMaterial ?? default );
//					File.WriteAllText( FileSystem.Game.GetAbsolutePath( CurrentPath, true, true ), data );
//				}
//			)
//		};

//		foreach ( var item in items )
//		{
//			ImGui.SameLine();

//			if ( item.text == "separator" )
//			{
//				ImGui.SeparatorV();
//			}
//			else
//			{
//				if ( ImGui.Button( item.text ) )
//				{
//					item.onClick();
//				}
//			}
//		}
//	}

//	private Texture GetTexture( string path )
//	{
//		if ( !TextureCache.ContainsKey( path ) )
//			TextureCache[path] = new Texture( path );

//		return TextureCache[path];
//	}

//	public override void Draw()
//	{
//		if ( ImGui.Begin( "Material Editor" ) )
//		{
//			DrawToolBar();

//			if ( ImGui.BeginTable( "##material_editor", 3, 0 ) )
//			{
//				ImGui.TableSetupFixedColumn( "Materials", 128.0f );
//				ImGui.TableSetupFixedColumn( "Properties", 256.0f );
//				ImGui.TableSetupStretchColumn( "Preview" );

//				ImGui.TableNextRow();
//				ImGui.TableNextColumn();

//				//
//				//
//				//
//				ImGui.BeginChild( "##materials", -1, -1 );

//				{
//					ImGui.Text( "Materials" );
//					ImGui.SeparatorH();

//					for ( int i = 0; i < LoadedMaterials.Count; ++i )
//					{
//						var materialName = LoadedMaterials[i].Path;
//						if ( ImGui.Selectable( materialName ) )
//						{
//							CurrentMaterialIndex = i;
//						}
//					}
//				}

//				ImGui.EndChild();
//				ImGui.TableNextColumn();

//				//
//				//
//				//
//				ImGui.BeginChild( "##material_properties", -1, -1 );

//				{
//					ImGui.Text( "Properties" );
//					ImGui.SeparatorH();

//					if ( CurrentMaterial == null )
//					{
//						ImGui.Text( "No material selected." );
//					}
//					else
//					{
//						DrawMaterialProperties();
//					}
//				}

//				ImGui.EndChild();
//				ImGui.TableNextColumn();

//				//
//				//
//				//
//				ImGui.BeginChild( "##material_preview", -1, -1 );

//				{
//					ImGui.Text( "Preview" );
//					ImGui.SeparatorH();

//					if ( CurrentMaterial != null && CurrentMaterial?.DiffuseTexture != null )
//					{
//						var width = ImGui.GetColumnWidth();
//						ImGui.Image( GetTexture( CurrentMaterial?.DiffuseTexture ).NativeTexture, (int)width, (int)width );
//					}
//				}

//				ImGui.EndChild();
//				ImGui.EndTable();
//			}
//		}

//		ImGui.End();
//	}
//}
