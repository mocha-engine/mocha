using Mocha.Common.Serialization;
using System.Text.Json;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".mmdl" } )]
public class ModelCompiler : BaseCompiler
{
	public override string CompileFile( string path )
	{
		Log.Processing( "Model", path );

		var destFileName = Path.ChangeExtension( path, "mmdl_c" );

		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var fileBinaryWriter = new BinaryWriter( fileStream );
		fileBinaryWriter.Write( new char[] { 'M', 'M', 'S', 'H' } ); // Magic number

		//
		// File header
		//
		fileBinaryWriter.Write( 4 ); // Version major
		fileBinaryWriter.Write( 0 ); // Version minor

		// Load json
		var fileData = File.ReadAllText( path );
		var modelData = JsonSerializer.Deserialize<ModelInfo>( fileData );

		var meshes = Primitives.Assimp.GenerateModels( Directory.GetCurrentDirectory(), modelData );

		fileBinaryWriter.Write( 0 ); // Pad
		fileBinaryWriter.Write( meshes.Count ); // Mesh count

		//
		// Set up compressed file body
		//
		using var compressedStream = new MemoryStream();
		using var compressedBinaryWriter = new BinaryWriter( compressedStream );

		//
		// Mesh list
		//
		foreach ( var mesh in meshes )
		{
			//
			// Material chunk
			//
			compressedBinaryWriter.Write( new char[] { 'M', 'T', 'R', 'L' } );
			compressedBinaryWriter.Write( mesh.Material );

			//
			// Vertex chunk
			//
			compressedBinaryWriter.Write( new char[] { 'V', 'R', 'T', 'X' } );
			compressedBinaryWriter.Write( mesh.Vertices.Length );

			foreach ( var vertex in mesh.Vertices )
			{
				void WriteVector3( Vector3 a )
				{
					// binaryWriter.Write( 0 );
					compressedBinaryWriter.Write( a.X );
					compressedBinaryWriter.Write( a.Y );
					compressedBinaryWriter.Write( a.Z );
				}

				void WriteVector2( Vector2 a )
				{
					// binaryWriter.Write( 0 );
					// binaryWriter.Write( 0 );
					compressedBinaryWriter.Write( a.X );
					compressedBinaryWriter.Write( a.Y );
				}

				WriteVector3( vertex.Position );
				WriteVector3( vertex.Normal );
				WriteVector2( vertex.TexCoords );
				WriteVector3( vertex.Tangent );
				WriteVector3( vertex.Bitangent );
			}

			//
			// Index chunk
			//
			compressedBinaryWriter.Write( new char[] { 'I', 'N', 'D', 'X' } );
			compressedBinaryWriter.Write( mesh.Indices.Length );

			foreach ( var index in mesh.Indices )
			{
				compressedBinaryWriter.Write( index );
			}
		}

		fileBinaryWriter.Write( Serializer.Compress( compressedStream.ToArray() ) );

		return destFileName;
	}
}
