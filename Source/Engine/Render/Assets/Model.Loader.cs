using Mocha.Common.Serialization;

namespace Mocha.Renderer;

partial class Model
{
	private void LoadFromPath( string path )
	{
		using var _ = new Stopwatch( "Mocha model generation" );
		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var modelFile = Serializer.Deserialize<MochaFile<byte[]>>( fileBytes );

		using var stream = new MemoryStream( modelFile.Data );
		using var binaryReader = new BinaryReader( stream );

		binaryReader.ReadChars( 4 ); // MMSH

		var verMajor = binaryReader.ReadInt32();
		var verMinor = binaryReader.ReadInt32();

		Log.Trace( $"Mocha model {verMajor}.{verMinor}" );

		binaryReader.ReadInt32(); // Pad

		var meshCount = binaryReader.ReadInt32();

		Log.Trace( $"{meshCount} meshes" );

		for ( int i = 0; i < meshCount; i++ )
		{
			binaryReader.ReadChars( 4 ); // MTRL

			var materialPath = binaryReader.ReadString();

			binaryReader.ReadChars( 4 ); // VRTX

			var vertexCount = binaryReader.ReadInt32();
			var vertices = new List<Vertex>();

			for ( int j = 0; j < vertexCount; j++ )
			{
				var vertex = new Vertex();

				Vector3 ReadVector3()
				{
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					float z = binaryReader.ReadSingle();
					return new Vector3( x, y, z );
				}

				Vector2 ReadVector2()
				{
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					return new Vector2( x, y );
				}

				vertex.Position = ReadVector3();
				vertex.Normal = ReadVector3();
				vertex.UV = ReadVector2();
				vertex.Tangent = ReadVector3();
				vertex.Bitangent = ReadVector3();

				vertices.Add( vertex );
			}

			binaryReader.ReadChars( 4 ); // INDX

			var indexCount = binaryReader.ReadInt32();
			var indices = new List<uint>();

			for ( int j = 0; j < indexCount; j++ )
			{
				indices.Add( binaryReader.ReadUInt32() );
			}

			Path = path;
			var material = new Material( materialPath );
			AddMesh( vertices.ToArray(), indices.ToArray(), material );
		}
	}

	private static Texture LoadMaterialTexture( string typeName, string path )
	{
		if ( !path.StartsWith( "internal:" ) )
		{
			path = path.Replace( "_BaseColor", "" );
			path = path.Remove( path.LastIndexOf( "." ) );
			path = path + $"_{typeName}.mtex";
		}

		if ( !FileSystem.Game.Exists( path ) )
		{
			Log.Warning( $"No texture '{path}'" );
			return Texture.MissingTexture;
		}

		using var _ = new Stopwatch( $"{typeName}: {path} texture load" );
		return new Texture( path );
	}
}
