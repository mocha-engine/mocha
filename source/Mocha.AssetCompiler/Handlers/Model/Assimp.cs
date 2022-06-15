using Assimp;

namespace Mocha;

partial class Primitives
{
	public static class Assimp
	{
		public static List<Model> GenerateModels( string path )
		{
			var models = new List<Model>();
			var context = new AssimpContext();
			var logStream = new LogStream( ( msg, _ ) => Console.WriteLine( msg ) );
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
				| PostProcessSteps.FlipWindingOrder
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
			List<VertexInfo> vertices = new List<VertexInfo>();
			List<uint> indices = new List<uint>();

			var material = new Material();

			for ( int i = 0; i < mesh.VertexCount; ++i )
			{
				var vertex = new VertexInfo()
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
					vertex.Tangent = new Vector3( mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z );
					vertex.Bitangent = new Vector3( mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z );
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

				material.DiffuseTexturePath = GetMaterialTexture( assimpMaterial, TextureType.Diffuse, "texture_diffuse", directory );
				material.SpecularTexturePath = GetMaterialTexture( assimpMaterial, TextureType.Specular, "texture_specular", directory );
				material.NormalTexturePath = GetMaterialTexture( assimpMaterial, TextureType.Normals, "texture_normal", directory );
				material.EmissiveTexturePath = GetMaterialTexture( assimpMaterial, TextureType.Emissive, "texture_emissive", directory );
				material.ORMTexturePath = GetMaterialTexture( assimpMaterial, TextureType.Unknown, "texture_unknown", directory );
			}

			return new Model( vertices.ToArray(), indices.ToArray(), material );
		}

		private static string GetMaterialTexture( global::Assimp.Material material, global::Assimp.TextureType textureType, string typeName, string? directory )
		{
			if ( material.GetMaterialTexture( textureType, 0, out var textureSlot ) )
			{
				return Path.Join( directory, textureSlot.FilePath );
			}
			else
			{
				return "internal:missing";
			}
		}
	}
}
