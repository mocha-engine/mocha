namespace Mocha;

partial class Primitives
{
	public class MochaModel
	{
		public static List<Model> GenerateModels( string path )
		{
			using var _ = new Stopwatch( "Mocha model generation" );
			using var fileStream = new FileStream( path, FileMode.Open, FileAccess.Read );
			using var binaryReader = new BinaryReader( fileStream );

			var models = new List<Model>();

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

				var material = new Material
				{
					Shader = ShaderBuilder.Default
										  .FromMoyaiShader( "content/shaders/pbr.mshdr" )
										  .Build(),
					UniformBufferType = typeof( GenericModelUniformBuffer )
				};

				var baseTexture = binaryReader.ReadString();
				binaryReader.ReadString();
				binaryReader.ReadString();
				binaryReader.ReadString();
				binaryReader.ReadString();

				material.DiffuseTexture = LoadMaterialTexture( "BaseColor", baseTexture );
				material.SpecularTexture = LoadMaterialTexture( "Roughness", baseTexture );
				material.NormalTexture = LoadMaterialTexture( "Normal", baseTexture );
				material.EmissiveTexture = TextureBuilder.MissingTexture;// LoadMaterialTexture( "BaseColor", baseTexture );
				material.ORMTexture = LoadMaterialTexture( "Metalness", baseTexture );

				binaryReader.ReadChars( 4 ); // VRTX

				var vertexCount = binaryReader.ReadInt32();
				var vertices = new List<Vertex>();

				for ( int j = 0; j < vertexCount; j++ )
				{
					var vertex = new Vertex();

					Vector3 ReadVector3()
					{
						binaryReader.ReadInt32();
						float x = binaryReader.ReadSingle();
						float y = binaryReader.ReadSingle();
						float z = binaryReader.ReadSingle();
						return new Vector3( x, y, z );
					}

					Vector2 ReadVector2()
					{
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
						float x = binaryReader.ReadSingle();
						float y = binaryReader.ReadSingle();
						return new Vector2( x, y );
					}

					vertex.Position = ReadVector3();
					vertex.Normal = ReadVector3();
					vertex.TexCoords = ReadVector2();
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

				models.Add( new Model( vertices.ToArray(), indices.ToArray(), material ) );
			}

			return models;
		}
		private static Texture LoadMaterialTexture( string typeName, string path )
		{
			if ( !path.StartsWith( "internal:" ) )
				path = Path.ChangeExtension( path, "mtex" );

			path = path.Replace( "BaseColor", typeName );

			if ( !File.Exists( path ) )
			{
				Log.Warning( $"No texture '{path}'" );
				return TextureBuilder.MissingTexture;
			}

			using var _ = new Stopwatch( $"{typeName}: {path} texture load" );
			return Texture.Builder
				.FromMochaTexture( path )
				.WithType( typeName )
				.Build();
		}
	}
}
