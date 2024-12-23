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

	/// <summary>
	/// Compiles a shader from GLSL into SPIR-V using Veldrid's libshaderc bindings.
	/// </summary>
	/// <returns>Vulkan-compatible SPIR-V bytecode.</returns>
	private void CompileShader( string? commonSource, string shaderSource, ShaderType shaderType, out ShaderReflectionInfo reflectionInfo, out int[] shaderCode )
	{
		//
		// Prepend a preamble with GLSL version & macro definitions
		//
		var preamble = new StringBuilder();

		preamble.AppendLine( commonSource );
		preamble.AppendLine();

		shaderSource = preamble.ToString() + shaderSource;

		//
		// Shader source data
		//
		var shaderResult = CompileShader( shaderType, shaderSource );
		var shaderData = shaderResult.ShaderData;
		shaderCode = new int[shaderData.count];
		Marshal.Copy( shaderData.data, shaderCode, 0, shaderCode.Length );

		//
		// Shader reflection info
		//
		reflectionInfo = shaderResult.ReflectionData.ToManaged();
	}

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		byte[] shaderBytes = input.SourceData.ToArray();
		string shaderString = Encoding.Default.GetString( shaderBytes );

		var shaderParser = new ShaderParser( shaderString );
		var shaderFile = shaderParser.Parse();

		var shaderFormat = new ShaderInfo();

		// Prepend attributes.mshdri (horrible hack)
		var attributeSource = File.ReadAllText( Path.GetDirectoryName( input.SourcePath ) + "\\attributes.mshdri" );
		var common = (shaderFile.Common ?? "") + "\n\n" + attributeSource;

		if ( shaderFile.Vertex != null )
		{
			CompileShader( common, shaderFile.Vertex, ShaderType.Vertex, out var reflection, out var data );
			shaderFormat.Vertex = new()
			{
				Data = data,
				Reflection = reflection
			};
		}

		if ( shaderFile.Fragment != null )
		{
			CompileShader( common, shaderFile.Fragment, ShaderType.Fragment, out var reflection, out var data );
			shaderFormat.Fragment = new()
			{
				Data = data,
				Reflection = reflection
			};
		}

		// Wrapper for file.
		var mochaFile = new MochaFile<ShaderInfo>
		{
			MajorVersion = 6,
			MinorVersion = 0,
			Data = shaderFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
