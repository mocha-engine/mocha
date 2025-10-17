namespace Mocha;

public class Cast
{
	private readonly List<BaseEntity> _ignoredEntities = new();
	private Common.TraceInfo _info;

	public static Cast Ray( Ray ray, float distance )
	{
		var endPosition = ray.startPosition + ray.direction * distance;

		return Cast.Ray( ray.startPosition, endPosition );
	}

	public static Cast Ray( Vector3 startPosition, Vector3 endPosition )
	{
		var trace = new Cast();
		trace._info.startPosition = startPosition;
		trace._info.endPosition = endPosition;

		trace._info.isBox = false;
		trace._info.extents = new();

		return trace;
	}

	public static Cast Box( Vector3 startPosition, Vector3 endPosition, Vector3 halfExtents )
	{
		var trace = new Cast();
		trace._info.startPosition = startPosition;
		trace._info.endPosition = endPosition;

		trace._info.isBox = true;
		trace._info.extents = halfExtents;

		return trace;
	}

	public Cast()
	{
		_info = new();
	}

	public Cast WithHalfExtents( Vector3 extents )
	{
		_info.isBox = true;
		_info.extents = extents;

		return this;
	}

	public Cast Ignore( ModelEntity entityToIgnore )
	{
		_ignoredEntities.Add( entityToIgnore );
		return this;
	}

	public TraceResult Run()
	{
		var traceInfo = _info;
		traceInfo.ignoredEntityCount = _ignoredEntities.Count;

		unsafe
		{
			fixed ( void* data = _ignoredEntities.Select( x => x.NativeHandle ).ToArray() )
			{
				traceInfo.ignoredEntityHandles = (IntPtr)data;

				var physicsManager = NativeEngine.GetPhysicsManager();
				var result = physicsManager.Trace( traceInfo );
				return TraceResult.From( result );
			}
		}
	}
}
