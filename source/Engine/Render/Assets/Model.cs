using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Cube ), Title( "Model" )]
public class Model : Asset
{
	public Material Material { get; set; }
	public bool IsIndexed { get; private set; }

	public Model( string path, Material material, bool isIndexed )
	{
		Path = path;
		Material = material;
		IsIndexed = isIndexed;

		All.Add( this );

		Material.Shader.OnRecompile += CreateResources;
	}

	public Model( string path, Vertex[] vertices, uint[] indices, Material material ) : this( path, material, true )
	{
		SetupMesh( vertices, indices );
		CreateUniformBuffer();
		CreateResources();
	}

	public Model( string path, Vertex[] vertices, Material material ) : this( path, material, false )
	{
		SetupMesh( vertices );
		CreateUniformBuffer();
		CreateResources();
	}

	private void SetupMesh( Vertex[] vertices )
	{
	}

	private void SetupMesh( Vertex[] vertices, uint[] indices )
	{
	}

	private void CreateResources()
	{
	}

	private void CreateUniformBuffer()
	{
	}
}
