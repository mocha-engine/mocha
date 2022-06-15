namespace Mocha;

public struct MochaFile<T>
{
	//
	// Compile info
	//
	public int MajorVersion { get; set; }
	public int MinorVersion { get; set; }

	//
	// Original asset info
	//
	public byte[] AssetHash { get; set; }

	//
	// Contents
	//
	public T Data { get; set; }
}
