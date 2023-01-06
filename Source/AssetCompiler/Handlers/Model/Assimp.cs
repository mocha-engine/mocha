using Assimp;

namespace Mocha.AssetCompiler;

public static class Assimp
{
	public static List<Model> GenerateModels( ModelInfo modelInfo )
	{
		var models = new List<Model>();
		var context = new AssimpContext();
		var logStream = new LogStream( ( msg, _ ) => Console.WriteLine( msg ) );

		// HACK: Specify content dir specifically for now.
		// This is done so that we can match the directory structure for models
		// with everything else - this is a temporary solution.
		var sourcePath = ("content\\" + modelInfo.Model).NormalizePath();
		var directory = Path.GetDirectoryName( sourcePath );

		var scene = context.ImportFile( sourcePath,
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

		if ( mesh.MaterialIndex >= 0 )
			material = modelInfo.Materials[mesh.MaterialIndex];

		return new Model( vertices.ToArray(), indices.ToArray(), material );
	}
}
