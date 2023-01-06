using Mocha.Common.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".mmat" } )]
public class MaterialCompiler : BaseCompiler
{
	public override string AssetName => "Material";

	public override CompileResult CompileFile( string path )
	{
		var destFileName = Path.ChangeExtension( path, "mmat_c" );

		// Load json
		var fileData = File.ReadAllText( path );
		var materialData = JsonSerializer.Deserialize<MaterialInfo>( fileData );

		// Wrapper for file
		var mochaFile = new MochaFile<MaterialInfo>()
		{
			MajorVersion = 3,
			MinorVersion = 1,
			Data = materialData
		};

		// Calculate original asset hash
		using ( var md5 = MD5.Create() )
			mochaFile.AssetHash = md5.ComputeHash( Encoding.Default.GetBytes( fileData ) );

		// Write result
		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( Serializer.Serialize( mochaFile ) );
		return Succeeded( path, destFileName );
	}
}
