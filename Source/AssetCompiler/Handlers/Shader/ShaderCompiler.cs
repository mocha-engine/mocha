using Mocha.Common.Serialization;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Mocha.AssetCompiler;

[Handles( ".mshdr" )]
public partial class ShaderCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Shader";

	/// <inheritdoc/>
	public override string CompiledExtension => "mshdr_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	/// <summary>
	/// Compiles a shader from GLSL into SPIR-V using Veldrid's libshaderc bindings.
	/// </summary>
	/// <returns>Vulkan-compatible SPIR-V bytecode.</returns>
	private int[] CompileShader( string shaderSource, ShaderStages shaderStage, string debugName = "temp" )
	{
		//
		// Prepend a preamble with GLSL version & macro definitions
		//
		var preamble = new StringBuilder();
		preamble.AppendLine( $"#version 460" );

		var shaderStageMacro = shaderStage == ShaderStages.Vertex ? "VERTEX" : "FRAGMENT";
		preamble.AppendLine( $"#define {shaderStageMacro}" );

		preamble.AppendLine();
		shaderSource = preamble.ToString() + shaderSource;

		//
		// Perform the compilation
		//
		var compileOptions = new GlslCompileOptions( false );
		var compileResult = SpirvCompilation.CompileGlslToSpirv( shaderSource, $"{debugName}_{shaderStageMacro}.glsl", shaderStage, compileOptions );

		// Data will be in bytes, but we want it in 32-bit integers as that is what Vulkan expects
		var dataBytes = compileResult.SpirvBytes;
		var dataInts = new int[dataBytes.Length / 4];
		Buffer.BlockCopy( dataBytes, 0, dataInts, 0, dataBytes.Length );

		return dataInts;
	}

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		byte[] shaderBytes = input.SourceData.ToArray();
		string shaderString = Encoding.Default.GetString( shaderBytes );

		// Debug name is used for error messages and internally by the SPIR-V compiler.
		var debugName = Path.GetFileNameWithoutExtension( input.SourcePath ) ?? "temp";

		var shaderFormat = new ShaderInfo()
		{
			VertexShaderData = CompileShader( shaderString, ShaderStages.Vertex, debugName ),
			FragmentShaderData = CompileShader( shaderString, ShaderStages.Fragment, debugName )
		};

		// Wrapper for file.
		var mochaFile = new MochaFile<ShaderInfo>
		{
			MajorVersion = 3,
			MinorVersion = 1,
			Data = shaderFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
