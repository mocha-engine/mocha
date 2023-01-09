using System.Text;
using System.Text.Json;

namespace Mocha.AssetCompiler;

/// <summary>
/// A compiler for .mmdl model files.
/// </summary>
[Handles( ".mmdl" )]
public class ModelCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Model";

	/// <inheritdoc/>
	public override string CompiledExtension => "mmdl_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => false;

	private static readonly char[] magicNumber = new char[] { 'M', 'M', 'S', 'H' };
	private static readonly char[] materialChunk = new char[] { 'M', 'T', 'R', 'L' };
	private static readonly char[] vertexChunk = new char[] { 'V', 'R', 'T', 'X' };
	private static readonly char[] indexChunk = new char[] { 'I', 'N', 'D', 'X' };

	/// <inheritdoc/>
	public override CompileResult CompileFile( ref CompileInput input )
	{
		// TODO: Fix this
		if ( input.SourcePath is null )
			throw new NotSupportedException( "Compiling a model requires compiling files on disk" );

		using var stream = new MemoryStream();
		using var binaryWriter = new BinaryWriter( stream );

		binaryWriter.Write( magicNumber ); // Magic number

		//
		// File header
		//
		binaryWriter.Write( 2 ); // Version major
		binaryWriter.Write( 0 ); // Version minor

		// Load json
		var modelData = JsonSerializer.Deserialize<ModelInfo>( Encoding.UTF8.GetString( input.SourceData.Span ) );

		var meshes = Assimp.GenerateModels( modelData );

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
			binaryWriter.Write( materialChunk );

			binaryWriter.Write( mesh.Material );

			//
			// Vertex chunk
			//
			binaryWriter.Write( vertexChunk );

			binaryWriter.Write( mesh.Vertices.Length );

			foreach ( var vertex in mesh.Vertices )
			{
				void WriteVector3( Vector3 a )
				{
					// binaryWriter.Write( 0 );
					binaryWriter.Write( a.X );
					binaryWriter.Write( a.Y );
					binaryWriter.Write( a.Z );
				}

				void WriteVector2( Vector2 a )
				{
					// binaryWriter.Write( 0 );
					// binaryWriter.Write( 0 );
					binaryWriter.Write( a.X );
					binaryWriter.Write( a.Y );
				}

				WriteVector3( vertex.Position * new System.Numerics.Vector3( -1, 1, 1 ) );
				WriteVector3( vertex.Normal * new System.Numerics.Vector3( -1, 1, 1 ) );
				WriteVector2( vertex.TexCoords * new System.Numerics.Vector2( -1, 1 ) );
				WriteVector3( vertex.Tangent * new System.Numerics.Vector3( -1, 1, 1 ) );
				WriteVector3( vertex.Bitangent * new System.Numerics.Vector3( -1, 1, 1 ) );
			}

			//
			// Index chunk
			//
			binaryWriter.Write( indexChunk );

			binaryWriter.Write( mesh.Indices.Length );

			foreach ( var index in mesh.Indices )
				binaryWriter.Write( index );
		}

		return Succeeded( stream.ToArray() );
	}
}
