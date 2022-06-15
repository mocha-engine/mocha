namespace Mocha;

public struct MeshInfo
{
	public string MaterialPath { get; set; }
	public byte[] MeshData { get; set; }
}

public struct ModelInfo
{
	public List<MeshInfo> Meshes { get; set; }
}
