using Assimp;

namespace MochaTool.AssetCompiler;

public static class Assimp
{
	public static List<Model> GenerateModels( ModelInfo modelInfo )
	{
		var models = new List<Model>();
		var context = new AssimpContext();
		var logStream = new LogStream( ( msg, _ ) => Console.WriteLine( msg ) );

		var sourceData = FileSystem.Mounted.ReadAllBytes( modelInfo.Model, FileSystemOptions.AssetCompiler );
		using var memoryStream = new MemoryStream( sourceData );

		var scene = context.ImportFileFromStream( memoryStream,
			PostProcessSteps.Triangulate
			| PostProcessSteps.RemoveRedundantMaterials
			| PostProcessSteps.CalculateTangentSpace
			| PostProcessSteps.GenerateSmoothNormals
			| PostProcessSteps.OptimizeMeshes
			| PostProcessSteps.OptimizeGraph
			| PostProcessSteps.ValidateDataStructure
			| PostProcessSteps.GenerateNormals
			| PostProcessSteps.FlipWindingOrder
			| PostProcessSteps.FlipUVs );

		ProcessNode( ref models, scene.RootNode, scene, modelInfo );

		return models;
	}

	private static void ProcessNode( ref List<Model> models, Node node, Scene scene, ModelInfo modelInfo )
	{
		for ( int i = 0; i < node.MeshCount; ++i )
		{
			var mesh = scene.Meshes[node.MeshIndices[i]];
			models.Add( ProcessMesh( mesh, scene, node.Transform, modelInfo ) );
		}

		foreach ( var child in node.Children )
		{
			ProcessNode( ref models, child, scene, modelInfo );
		}
	}

	private static Model ProcessMesh( Mesh mesh, Scene scene, Matrix4x4 transform, ModelInfo modelInfo )
	{
		List<VertexInfo> vertices = new List<VertexInfo>();
		List<uint> indices = new List<uint>();

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

		string material = "internal:missing";

		var materialSearchName = scene.Materials[mesh.MaterialIndex].Name;
		var searchPaths = new[]
		{
			$"materials/{materialSearchName}.mmat",
			$"materials/{materialSearchName}/{materialSearchName}.mmat",
			$"textures/{materialSearchName}/{materialSearchName}.mmat",
			$"textures/{materialSearchName}.mmat",
		};
		var materialWasFound = false;

		foreach ( var searchPath in searchPaths )
		{
			if ( FileSystem.Mounted.Exists( searchPath ) )
			{
				material = searchPath;
				materialWasFound = true;
				break;
			}
		}

		if ( !materialWasFound && mesh.MaterialIndex >= 0 && mesh.MaterialIndex < modelInfo.Materials.Count )
			material = modelInfo.Materials[mesh.MaterialIndex];

		return new Model( vertices.ToArray(), indices.ToArray(), material );
	}
}
