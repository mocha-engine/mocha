namespace Mocha;

public struct TraceResult
{
	public bool Hit;
	public Vector3 StartPosition;
	public Vector3 EndPosition;
	public float Fraction;
	public Vector3 Normal;

	public bool StartedSolid;
	public bool EndedSolid;

	public static TraceResult From( Common.TraceResult orig )
	{
		return new TraceResult()
		{
			Hit = orig.hit,
			StartPosition = orig.startPosition,
			EndPosition = orig.endPosition,
			Fraction = orig.fraction,
			Normal = orig.normal,
			StartedSolid = orig.startedSolid,
			EndedSolid = orig.endedSolid,
		};
	}
}
