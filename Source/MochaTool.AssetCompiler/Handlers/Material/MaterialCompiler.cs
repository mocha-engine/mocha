using System.Text.Json;

namespace MochaTool.AssetCompiler;

/// <summary>
/// A compiler for .mmat material files.
/// </summary>
[Handles( ".mmat" )]
public class MaterialCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Material";

	/// <inheritdoc/>
	public override string CompiledExtension => "mmat_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		var materialData = JsonSerializer.Deserialize<MaterialInfo>( input.SourceData.Span );
		// Wrapper for file
		var mochaFile = new MochaFile<MaterialInfo>()
		{
			MajorVersion = 4,
			MinorVersion = 0,
			Data = materialData,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
