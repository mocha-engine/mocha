using System.Reflection;
using System.Text.Json;

namespace Mocha.Editor;

public class MaterialEditorWindow : EditorWindow
{
	private MaterialInfo? CurrentMaterial { get; set; }
	private string CurrentPath { get; set; } = "unnamed.mmat";

	private Dictionary<string, Texture> TextureCache = new();

	private void DrawMaterialTexture( PropertyInfo propertyInfo )
	{
		var name = propertyInfo.Name;
		var currentPath = propertyInfo.GetValue( CurrentMaterial ) as string;

		ImGui.Text( name );
		ImGui.Text( currentPath ?? "None" );

		if ( currentPath != null && FileSystem.Game.Exists( currentPath ) )
		{
			if ( !TextureCache.ContainsKey( currentPath ) )
				TextureCache[currentPath] = new Texture( currentPath );

			ImGui.Image( TextureCache[currentPath].NativeTexture, 64, 64 );
		}

		if ( ImGui.Button( $"Replace...##{propertyInfo.GetHashCode()}" ) )
		{
			var openFileDialog = new OpenFileWindow();
			openFileDialog.Filter = "*.png, *.jpg";
			openFileDialog.OnSelected += ( path ) =>
			{
				path = Path.ChangeExtension( path, "mtex" );

				// SetValue does not work on non-boxed struct
				object boxed = CurrentMaterial;
				propertyInfo.SetValue( boxed, path );
				CurrentMaterial = (MaterialInfo)boxed;
			};

			openFileDialog.Show();
		}
	}

	private void DrawMaterial()
	{
		foreach ( var property in typeof( MaterialInfo ).GetProperties() )
		{
			ImGui.Separator();

			DrawMaterialTexture( property );
		}
	}

	private void DrawToolBar()
	{
		if ( ImGui.Button( "New" ) )
			CurrentMaterial = new();

		ImGui.SameLine();

		if ( ImGui.Button( "Open" ) )
		{
			var openFileDialog = new OpenFileWindow();
			openFileDialog.Filter = "*.mmat";
			openFileDialog.OnSelected += ( path ) =>
			{
				CurrentPath = path;

				var data = File.ReadAllText( FileSystem.Game.GetAbsolutePath( path, true, true ) );
				CurrentMaterial = JsonSerializer.Deserialize<MaterialInfo>( data );
			};

			openFileDialog.Show();
		}

		ImGui.SameLine();

		if ( ImGui.Button( "Save" ) )
		{
			var data = JsonSerializer.Serialize<MaterialInfo>( CurrentMaterial ?? default );
			File.WriteAllText( FileSystem.Game.GetAbsolutePath( CurrentPath, true, true ), data );
		}
	}

	public override void Draw()
	{
		if ( ImGui.Begin( "Material Editor" ) )
		{
			DrawToolBar();

			if ( CurrentMaterial == null )
			{
				ImGui.Text( "No material selected." );
			}
			else
			{
				DrawMaterial();
			}
		}

		ImGui.End();
	}
}
