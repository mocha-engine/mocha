using System.Security.Cryptography;
using Mocha.Common.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".mat" } )]
public class MaterialCompiler : BaseCompiler
{
	public override string CompileFile( string path )
	{
		Console.WriteLine( $"[MATERIAL]\t{path}" );

		var destFileName = Path.ChangeExtension( path, extension: "mmat" );

		// Load json
		var fileData = File.ReadAllText( path );
		var materialData = JsonConvert.DeserializeObject<MaterialInfo>( fileData );

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
		return destFileName;
	}
}
