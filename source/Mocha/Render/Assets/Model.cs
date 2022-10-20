using Mocha.Common.Serialization;

namespace Mocha.Renderer;
public class Model
{
	public List<Mesh> Meshes { get; } = new();

	public Model( params Mesh[] meshes )
	{
		Meshes.AddRange( meshes );
	}

	public Model( string path )
	{
		using var _ = new Stopwatch( "Mocha model generation" );
		using var fileStream = FileSystem.Game.OpenRead( path );
		using var binaryReader = new BinaryReader( fileStream );

		binaryReader.ReadChars( 4 ); // MMSH

		var verMajor = binaryReader.ReadInt32();
		var verMinor = binaryReader.ReadInt32();

		if ( verMajor != 4 && verMinor != 0 )
			throw new Exception( $"Unsupported MMDL file version {verMajor}.{verMinor}" );

		Log.Trace( $"Mocha model {verMajor}.{verMinor}" );

		binaryReader.ReadInt32(); // Pad
		var meshCount = binaryReader.ReadInt32();

		Log.Trace( $"{meshCount} meshes" );

		//
		// Decompress the rest of the file
		//
		var compressedData = binaryReader.ReadBytes( (int)(fileStream.Length - fileStream.Position) );
		var decompressedData = Serializer.Decompress( compressedData );

		var decompressedStream = new MemoryStream( decompressedData );
		var decompressedBinaryReader = new BinaryReader( decompressedStream );

		for ( int i = 0; i < meshCount; i++ )
		{
			decompressedBinaryReader.ReadChars( 4 ); // MTRL
			var materialPath = decompressedBinaryReader.ReadString();

			decompressedBinaryReader.ReadChars( 4 ); // VRTX
			var vertexCount = decompressedBinaryReader.ReadInt32();
			var vertices = new List<Vertex>();

			for ( int j = 0; j < vertexCount; j++ )
			{
				var vertex = new Vertex();

				Vector3 ReadVector3()
				{
					// binaryReader.ReadInt32();
					float x = decompressedBinaryReader.ReadSingle();
					float y = decompressedBinaryReader.ReadSingle();
					float z = decompressedBinaryReader.ReadSingle();
					return new Vector3( x, y, z );
				}

				Vector2 ReadVector2()
				{
					// binaryReader.ReadInt32();
					// binaryReader.ReadInt32();
					float x = decompressedBinaryReader.ReadSingle();
					float y = decompressedBinaryReader.ReadSingle();
					return new Vector2( x, y );
				}

				vertex.Position = ReadVector3();
				vertex.Normal = ReadVector3();
				vertex.TexCoords = ReadVector2();
				vertex.Tangent = ReadVector3();
				vertex.Bitangent = ReadVector3();

				vertices.Add( vertex );
			}

			decompressedBinaryReader.ReadChars( 4 ); // INDX

			var indexCount = decompressedBinaryReader.ReadInt32();
			var indices = new List<uint>();

			for ( int j = 0; j < indexCount; j++ )
			{
				indices.Add( decompressedBinaryReader.ReadUInt32() );
			}

			// TODO make all paths relative
			var material = new Material( materialPath );
			Meshes.Add( new Mesh( path, vertices.ToArray(), indices.ToArray(), material ) );
		}
	}
}
