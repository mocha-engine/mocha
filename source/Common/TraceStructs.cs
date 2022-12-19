using System.Runtime.InteropServices;

namespace Mocha.Glue;

[StructLayout( LayoutKind.Sequential )]
public struct TraceResult
{
	public bool hit;
	public Vector3 startPosition;
	public Vector3 endPosition;
	public float fraction;
	public Vector3 normal;
	public bool startedSolid;
	public bool endedSolid;
	public uint entityHandle;
	public uint pad0;
}

[StructLayout( LayoutKind.Sequential )]
public struct TraceInfo
{
	public Vector3 startPosition;
	public Vector3 endPosition;
	public bool isBox;
	public Vector3 extents;
	public int ignoredEntityCount;
	public IntPtr ignoredEntityHandles;
}

