using Mocha;
using Mocha.Glue;
using System.Runtime.InteropServices;
using System.Text;

namespace MochaTool.AssetCompiler;

[Handles( ".mshdr" )]
public partial class ShaderCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Shader";

	/// <inheritdoc/>
	public override string CompiledExtension => "mshdr_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	[DllImport( "MochaTool.ShaderCompilerBindings.dll", CharSet = CharSet.Ansi )]
	private static extern UtilArray CompileShader( ShaderType shaderType, string shaderSource );

	/// <summary>
	/// Compiles a shader from GLSL into SPIR-V using Veldrid's libshaderc bindings.
	/// </summary>
	/// <returns>Vulkan-compatible SPIR-V bytecode.</returns>
	private int[] CompileShader( string? commonSource, string shaderSource, ShaderType shaderType, string debugName = "temp" )
	{
		//
		// Prepend a preamble with GLSL version & macro definitions
		//
		var preamble = new StringBuilder();

		preamble.AppendLine( commonSource );
		preamble.AppendLine();

		shaderSource = preamble.ToString() + shaderSource;

		var shaderData = CompileShader( shaderType, shaderSource );
		var dataInts = new int[shaderData.count];
		Marshal.Copy( shaderData.data, dataInts, 0, dataInts.Length );

		return dataInts;
	}

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		byte[] shaderBytes = input.SourceData.ToArray();
		string shaderString = Encoding.Default.GetString( shaderBytes );

		var shaderParser = new ShaderParser( shaderString );
		var shaderFile = shaderParser.Parse();

		// Debug name is used for error messages and internally by the SPIR-V compiler.
		var debugName = Path.GetFileNameWithoutExtension( input.SourcePath ) ?? "temp";

		var shaderFormat = new ShaderInfo();

		if ( shaderFile.Vertex != null )
			shaderFormat.VertexShaderData = CompileShader( shaderFile.Common, shaderFile.Vertex, ShaderType.Vertex, debugName );

		if ( shaderFile.Fragment != null )
			shaderFormat.FragmentShaderData = CompileShader( shaderFile.Common, shaderFile.Fragment, ShaderType.Fragment, debugName );

		/*
		if ( shaderFile.Compute != null )
			shaderFormat.ComputeShaderData = CompileShader( shaderFile.Common, shaderFile.Compute, ShaderStages.Compute, debugName );
		*/

		// Wrapper for file.
		var mochaFile = new MochaFile<ShaderInfo>
		{
			MajorVersion = 4,
			MinorVersion = 0,
			Data = shaderFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
