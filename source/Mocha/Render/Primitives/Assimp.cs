using Assimp;
using System.Numerics;

namespace Mocha;

partial class Primitives
{
	public static class Assimp
	{
		private static Vector4 AssimpColorToVec4( Color4D col )
		{
			return new Vector4( col.R, col.G, col.B, col.A );
		}

		public static List<Model> GenerateModels( string path )
		{
			var models = new List<Model>();
			var context = new AssimpContext();
			var logStream = new LogStream( ( msg, _ ) => Log.Trace( msg ) );
			var directory = Path.GetDirectoryName( path );

			var scene = context.ImportFile( path,
				PostProcessSteps.Triangulate
				| PostProcessSteps.PreTransformVertices
				| PostProcessSteps.RemoveRedundantMaterials
				| PostProcessSteps.CalculateTangentSpace
				| PostProcessSteps.OptimizeMeshes
				| PostProcessSteps.OptimizeGraph
				| PostProcessSteps.ValidateDataStructure
				| PostProcessSteps.GenerateNormals
				| PostProcessSteps.FlipUVs );

			ProcessNode( ref models, scene.RootNode, scene, directory );

			return models;
		}

		private static void ProcessNode( ref List<Model> models, Node node, global::Assimp.Scene scene, string? directory )
		{
			for ( int i = 0; i < node.MeshCount; ++i )
			{
				var mesh = scene.Meshes[node.MeshIndices[i]];
				models.Add( ProcessMesh( mesh, scene, node.Transform, directory ) );
			}

			foreach ( var child in node.Children )
			{
				ProcessNode( ref models, child, scene, directory );
			}
		}

		private static Model ProcessMesh( global::Assimp.Mesh mesh, global::Assimp.Scene scene, global::Assimp.Matrix4x4 transform, string? directory )
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();

			var material = new Material
			{
				Shader = ShaderBuilder.Default
									  .WithVertex( "content/shaders/3d/3d.vert" )
									  .WithFragment( "content/shaders/3d/3d.frag" )
									  .Build(),
				UniformBufferType = typeof( GenericModelUniformBuffer )
			};

			for ( int i = 0; i < mesh.VertexCount; ++i )
			{
				var vertex = new Vertex()
				{
					Position = new Vector3( mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z ),
					Normal = new Vector3( mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z )
				};

				if ( mesh.HasTextureCoords( 0 ) )
				{
					var texCoords = new Vector2( mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y );
					vertex.TexCoords = texCoords;
				}
				else
				{
					vertex.TexCoords = new Vector2( 0, 0 );
				}

				if ( mesh.HasTangentBasis )
				{
					// vertex.Tangent = new Vector3( mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z );
					// vertex.BiTangent = new Vector3( mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z );
				}

				vertices.Add( vertex );
			}

			for ( int i = 0; i < mesh.FaceCount; ++i )
			{
				var face = mesh.Faces[i];
				for ( int f = 0; f < face.IndexCount; ++f )
				{
					indices.Add( (uint)face.Indices[f] );
				}
			}

			if ( mesh.MaterialIndex >= 0 )
			{
				var assimpMaterial = scene.Materials[mesh.MaterialIndex];

				material.DiffuseTexture = LoadMaterialTexture( assimpMaterial, TextureType.Diffuse, "texture_diffuse", directory );
				material.SpecularTexture = LoadMaterialTexture( assimpMaterial, TextureType.Specular, "texture_specular", directory );
				material.NormalTexture = LoadMaterialTexture( assimpMaterial, TextureType.Normals, "texture_normal", directory );
				material.EmissiveTexture = LoadMaterialTexture( assimpMaterial, TextureType.Emissive, "texture_emissive", directory );
				material.ORMTexture = LoadMaterialTexture( assimpMaterial, TextureType.Unknown, "texture_unknown", directory );
			}

			var oglTransform = new System.Numerics.Matrix4x4(
				transform.A1, transform.A2, transform.A3, transform.A4,
				transform.B1, transform.B2, transform.B3, transform.B4,
				transform.C1, transform.C2, transform.C3, transform.C4,
				transform.D1, transform.D2, transform.D3, transform.D4
			);

			return new Model( vertices.ToArray(), indices.ToArray(), material );
		}

		private static Texture LoadMaterialTexture( global::Assimp.Material material, global::Assimp.TextureType textureType, string typeName, string? directory )
		{
			if ( material.GetMaterialTexture( textureType, 0, out var textureSlot ) )
			{
				return Texture.Builder
					.FromPath( Path.Join( directory, textureSlot.FilePath ) )
					.WithType( typeName )
					.Build();
			}
			else
			{
				return TextureBuilder.MissingTexture;
			}
		}
	}
}
