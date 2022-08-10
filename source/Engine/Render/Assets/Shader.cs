using System.Text;

namespace Mocha.Renderer;

[Icon( FontAwesome.Glasses ), Title( "Shader" )]
public class Shader : Asset
{
	private Glue.CShader NativeShader { get; }

	public Action OnRecompile { get; set; }
	public bool IsDirty { get; private set; }

	private FileSystemWatcher watcher;

	internal Shader( string path, string vertexSource, string fragmentSource )
	{
		All.Add( this );
		Path = path;

		NativeShader = new( vertexSource, fragmentSource );

		CreateWatcher();
		Compile();
	}

	private void CreateWatcher()
	{
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

	public void Compile()
	{
		if ( !IsFileReady( Path ) )
			return;

		Log.Info( $"Compiling shader {Path}" );
		Notify.AddNotification( $"Shader Compiling", FontAwesome.Ellipsis );

		if ( NativeShader.Compile() == 0 )
		{
			Notify.AddNotification( $"Shader Compilation Success!", $"Compiled shader {Path}", FontAwesome.FaceGrinStars );
			CreatePipelines();
			OnRecompile?.Invoke();
		}
		else
		{
			Notify.AddNotification( $"Shader Compilation Fail", FontAwesome.FaceSadCry );
		}

		IsDirty = false;
	}
}
