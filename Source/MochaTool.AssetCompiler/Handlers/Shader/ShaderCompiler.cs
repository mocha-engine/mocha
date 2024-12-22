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

	[StructLayout( LayoutKind.Sequential )]
	private struct ShaderCompilerResult
	{
		public UtilArray ShaderData;
		public NativeShaderReflectionInfo ReflectionData;
	}

	[DllImport( "MochaTool.ShaderCompilerBindings.dll", CharSet = CharSet.Ansi )]
	private static extern ShaderCompilerResult CompileShader( ShaderType shaderType, string shaderSource );

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
		reflectionInfo = new();

		var bindings = new ShaderReflectionBinding[shaderResult.ReflectionData.Bindings.count];
		for ( int i = 0; i < shaderResult.ReflectionData.Bindings.count; i++ )
		{
			bindings[i] = Marshal.PtrToStructure<ShaderReflectionBinding>( shaderResult.ReflectionData.Bindings.data + (i * Marshal.SizeOf<ShaderReflectionBinding>()) );
		}

		reflectionInfo.Bindings = bindings;
	}

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		byte[] shaderBytes = input.SourceData.ToArray();
		string shaderString = Encoding.Default.GetString( shaderBytes );

		var shaderParser = new ShaderParser( shaderString );
		var shaderFile = shaderParser.Parse();

		var shaderFormat = new ShaderInfo();

		if ( shaderFile.Vertex != null )
		{
			CompileShader( shaderFile.Common, shaderFile.Vertex, ShaderType.Vertex, out var reflection, out var data );
			shaderFormat.Vertex = new()
			{
				Data = data,
				Reflection = reflection
			};
		}

		if ( shaderFile.Fragment != null )
		{
			CompileShader( shaderFile.Common, shaderFile.Fragment, ShaderType.Fragment, out var reflection, out var data );
			shaderFormat.Fragment = new()
			{
				Data = data,
				Reflection = reflection
			};
		}

		/*
		if ( shaderFile.Compute != null )
			shaderFormat.ComputeShaderData = CompileShader( shaderFile.Common, shaderFile.Compute, ShaderStages.Compute, debugName );
		*/

		// Wrapper for file.
		var mochaFile = new MochaFile<ShaderInfo>
		{
			MajorVersion = 5,
			MinorVersion = 0,
			Data = shaderFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
