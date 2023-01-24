namespace Mocha;

public class Cast
{
	private readonly List<BaseEntity> IgnoredEntities = new();
	private Common.TraceInfo info;

	public static Cast Ray( Ray ray, float distance )
	{
		var endPosition = ray.startPosition + ray.direction * distance;

		return Cast.Ray( ray.startPosition, endPosition );
	}

	public static Cast Ray( Vector3 startPosition, Vector3 endPosition )
	{
		var trace = new Cast();
		trace.info.startPosition = startPosition;
		trace.info.endPosition = endPosition;

		trace.info.isBox = false;
		trace.info.extents = new();

		return trace;
	}

	public static Cast Box( Vector3 startPosition, Vector3 endPosition, Vector3 halfExtents )
	{
		var trace = new Cast();
		trace.info.startPosition = startPosition;
		trace.info.endPosition = endPosition;

		trace.info.isBox = true;
		trace.info.extents = halfExtents;

		return trace;
	}

	public Cast()
	{
		info = new();
	}

	public Cast WithHalfExtents( Vector3 extents )
	{
		info.isBox = true;
		info.extents = extents;

		return this;
	}

	public Cast Ignore( ModelEntity entityToIgnore )
	{
		IgnoredEntities.Add( entityToIgnore );
		return this;
	}

	public TraceResult Run()
	{
		var traceInfo = info;
		traceInfo.ignoredEntityCount = IgnoredEntities.Count;

		unsafe
		{
			fixed ( void* data = IgnoredEntities.Select( x => x.NativeHandle ).ToArray() )
			{
				traceInfo.ignoredEntityHandles = (IntPtr)data;
				var result = Glue.Physics.Trace( traceInfo );
				return TraceResult.From( result );
			}
		}
	}
}
