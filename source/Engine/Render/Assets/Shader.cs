namespace Mocha.Renderer;

[Icon( FontAwesome.Glasses ), Title( "Shader" )]
public class Shader : Asset
{
	private Glue.CShader NativeShader { get; }

	public IntPtr NativePtr => NativeShader.NativePtr;

	public Action OnRecompile { get; set; }
	public bool IsDirty { get; private set; }

	private FileSystemWatcher watcher;

	internal Shader( string path, string source )
	{
		All.Add( this );
		Path = path;

		NativeShader = new( Path, source );

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

		Notify.AddNotification( $"Shader Compiling", $"Compiling '{Path}'...", FontAwesome.Ellipsis );

		if ( NativeShader.Compile() )
		{
			Notify.AddNotification( $"Shader Compilation Success!", $"Compiled shader '{Path}'", FontAwesome.FaceGrinStars );
			CreatePipelines();
			OnRecompile?.Invoke();
		}
		else
		{
			Notify.AddNotification( $"Shader Compilation Fail", $"Failed to compile '{Path}'", FontAwesome.FaceSadCry );
		}

		IsDirty = false;
	}
}
