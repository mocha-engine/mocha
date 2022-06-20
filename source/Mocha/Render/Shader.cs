using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Mocha.Renderer;

public class Shader
{
	public static List<Shader> All { get; set; } = new();
	public Veldrid.Shader[] ShaderProgram { get; private set; }
	public string Path { get; set; }
	public Action OnRecompile { get; set; }

	public FileSystemWatcher Watcher;

	public bool IsDirty { get; private set; }

	internal Shader( string path, Veldrid.Shader[] shaderProgram )
	{
		All.Add( this );

		ShaderProgram = shaderProgram;
		Path = path;

		Log.Trace( $"shader ctor {Path}" );

		var directoryName = System.IO.Path.GetDirectoryName( Path );
		var fileName = System.IO.Path.GetFileName( Path );
		Watcher = new FileSystemWatcher( directoryName, fileName );

		Watcher.NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size;

		Watcher.Changed += OnWatcherChanged;
		Watcher.EnableRaisingEvents = true;
	}

	private void OnWatcherChanged( object sender, FileSystemEventArgs e )
	{
		IsDirty = true;
	}

	public static bool IsFileReady( string path )
	{
		try
		{
			using ( FileStream inputStream = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.None ) )
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

		var shaderText = File.ReadAllText( Path );

		var vertexShaderText = $"#version 450\n#define VERTEX\n{shaderText}";
		var fragmentShaderText = $"#version 450\n#define FRAGMENT\n{shaderText}";

		var vertexShaderBytes = Encoding.Default.GetBytes( vertexShaderText );
		var fragmentShaderBytes = Encoding.Default.GetBytes( fragmentShaderText );

		var vertexShaderDescription = new ShaderDescription( ShaderStages.Vertex, vertexShaderBytes, "main" );
		var fragmentShaderDescription = new ShaderDescription( ShaderStages.Fragment, fragmentShaderBytes, "main" );

		try
		{
			var fragCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( fragmentShaderDescription.ShaderBytes ),
				Path + "_FS",
				ShaderStages.Fragment,
				new GlslCompileOptions( debug: true ) );
			fragmentShaderDescription.ShaderBytes = fragCompilation.SpirvBytes;

			var vertCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( vertexShaderDescription.ShaderBytes ),
				Path + "_VS",
				ShaderStages.Vertex,
				new GlslCompileOptions( debug: true ) );
			vertexShaderDescription.ShaderBytes = vertCompilation.SpirvBytes;

			ShaderProgram = Device.ResourceFactory.CreateFromSpirv( vertexShaderDescription, fragmentShaderDescription );

			Notify.AddNotification( "Shader Compilation Success!", $"Compiled shader {Path}" );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Compile failed:\n{ex.Message}" );
			Notify.AddNotification( "Shader Compilation Fail", $"{ex.Message}" );
		}

		IsDirty = false;
		OnRecompile?.Invoke();
	}
}
