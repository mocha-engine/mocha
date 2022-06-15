namespace Mocha.AssetCompiler;

public class ModelCompiler
{
	public static string CompileFile( string path )
	{
		var destFileName = Path.ChangeExtension( path, "mmdl" );
		Console.WriteLine( $"Compiling {path}" );

		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( new char[] { 'M', 'M', 'S', 'H' } ); // Magic number

		//
		// File header
		//
		binaryWriter.Write( 1 ); // Version major
		binaryWriter.Write( 0 ); // Version minor

		var meshes = Primitives.Assimp.GenerateModels( path );

		binaryWriter.Write( 0 ); // Pad
		binaryWriter.Write( meshes.Count ); // Mesh count

		//
		// Mesh list
		//
		foreach ( var mesh in meshes )
		{
			//
			// Material chunk
			//
			binaryWriter.Write( new char[] { 'M', 'T', 'R', 'L' } );

			binaryWriter.Write( mesh.Material.DiffuseTexturePath );
			binaryWriter.Write( mesh.Material.SpecularTexturePath );
			binaryWriter.Write( mesh.Material.NormalTexturePath );
			binaryWriter.Write( mesh.Material.EmissiveTexturePath );
			binaryWriter.Write( mesh.Material.ORMTexturePath );

			//
			// Vertex chunk
			//
			binaryWriter.Write( new char[] { 'V', 'R', 'T', 'X' } );

			binaryWriter.Write( mesh.Vertices.Length );

			foreach ( var vertex in mesh.Vertices )
			{
				void WriteVector3( Vector3 a )
				{
					binaryWriter.Write( 0 );
					binaryWriter.Write( a.X );
					binaryWriter.Write( a.Y );
					binaryWriter.Write( a.Z );
				}

				void WriteVector2( Vector2 a )
				{
					binaryWriter.Write( 0 );
					binaryWriter.Write( 0 );
					binaryWriter.Write( a.X );
					binaryWriter.Write( a.Y );
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
			binaryWriter.Write( new char[] { 'I', 'N', 'D', 'X' } );

			binaryWriter.Write( mesh.Indices.Length );

			foreach ( var index in mesh.Indices )
			{
				binaryWriter.Write( index );
			}
		}

		return destFileName;
	}
}
