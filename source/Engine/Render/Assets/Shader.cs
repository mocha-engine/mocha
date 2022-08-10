using System.Text;

namespace Mocha.Renderer;

[Icon( FontAwesome.Glasses ), Title( "Shader" )]
public class Shader : Asset
{
	public Action OnRecompile { get; set; }
	public bool IsDirty { get; private set; }


	private FileSystemWatcher watcher;

	internal Shader( string path )
	{
		All.Add( this );
		Path = path;

		var directoryName = System.IO.Path.GetDirectoryName( Path );
		var fileName = System.IO.Path.GetFileName( Path );

		watcher = FileSystem.Game.CreateWatcher( directoryName, fileName );
		watcher.Changed += OnWatcherChanged;

		CreatePipelines();
	}

	private void CreatePipelines()
	{
	}

	private void OnWatcherChanged( object sender, FileSystemEventArgs e )
	{
		IsDirty = true;
	}

	public static bool IsFileReady( string path )
	{
		try
		{
			using ( FileStream inputStream = FileSystem.Game.OpenRead( path ) )
				return inputStream.Length > 0;
		}
		catch ( Exception )
		{
			return false;
		}
	}

	public void Recompile()
	{
		if ( !IsFileReady( Path ) )
			return;

		var shaderText = FileSystem.Game.ReadAllText( Path );

		var vertexShaderText = $"#version 450\n#define VERTEX\n{shaderText}";
		var fragmentShaderText = $"#version 450\n#define FRAGMENT\n{shaderText}";

		var vertexShaderBytes = Encoding.Default.GetBytes( vertexShaderText );
		var fragmentShaderBytes = Encoding.Default.GetBytes( fragmentShaderText );

		try
		{

			CreatePipelines();

			Notify.AddNotification( $"Shader Compilation Success!", $"Compiled shader {Path}", FontAwesome.FaceGrinStars );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Compile failed:\n{ex.Message}" );
			Notify.AddNotification( $"Shader Compilation Fail", $"{ex.Message}", FontAwesome.FaceSadCry );
		}

		IsDirty = false;
		OnRecompile?.Invoke();
	}
}
