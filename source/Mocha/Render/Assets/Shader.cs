using System.Text;
using Veldrid.SPIRV;

namespace Mocha.Renderer;

[Icon( FontAwesome.Glasses ), Title( "Shader" )]
public class Shader : Asset
{
	public Veldrid.Shader[] ShaderProgram { get; private set; }
	public PipelineFactory PipelineFactory { get; set; }
	public RenderPipeline Pipeline { get; set; }
	public Action OnRecompile { get; set; }
	public bool IsDirty { get; private set; }

	private Framebuffer TargetFramebuffer { get; set; }
	private FaceCullMode FaceCullMode { get; set; }

	private FileSystemWatcher watcher;

	internal Shader( string path, Framebuffer targetFramebuffer, FaceCullMode faceCullMode, Veldrid.Shader[] shaderProgram )
	{
		All.Add( this );

		ShaderProgram = shaderProgram;
		Path = path;

		var directoryName = System.IO.Path.GetDirectoryName( Path );
		var fileName = System.IO.Path.GetFileName( Path );

		watcher = FileSystem.Game.CreateWatcher( directoryName, fileName );
		watcher.Changed += OnWatcherChanged;

		this.TargetFramebuffer = targetFramebuffer;
		this.FaceCullMode = faceCullMode;
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

		var vertexShaderDescription = new ShaderDescription( ShaderStages.Vertex, vertexShaderBytes, "main" );
		var fragmentShaderDescription = new ShaderDescription( ShaderStages.Fragment, fragmentShaderBytes, "main" );

		try
		{
			var fragCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( fragmentShaderDescription.ShaderBytes ),
				Path + "_FS",
				ShaderStages.Fragment,
				new GlslCompileOptions( debug: false ) );
			fragmentShaderDescription.ShaderBytes = fragCompilation.SpirvBytes;

			var vertCompilation = SpirvCompilation.CompileGlslToSpirv(
				Encoding.UTF8.GetString( vertexShaderDescription.ShaderBytes ),
				Path + "_VS",
				ShaderStages.Vertex,
				new GlslCompileOptions( debug: false ) );
			vertexShaderDescription.ShaderBytes = vertCompilation.SpirvBytes;

			ShaderProgram = Device.ResourceFactory.CreateFromSpirv( vertexShaderDescription, fragmentShaderDescription );

			Notify.AddNotification( $"Shader Compilation Success!", $"Compiled shader {Path}", FontAwesome.FaceGrinStars );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Compile failed:\n{ex.Message}" );
			Notify.AddNotification( $"Shader Compilation Fail", $"{ex.Message}", FontAwesome.FaceSadCry );
		}

		Pipeline = PipelineFactory.Build();
		IsDirty = false;
		OnRecompile?.Invoke();
	}
}
