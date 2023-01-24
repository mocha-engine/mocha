namespace Mocha;

partial class UIEntity
{
	public override void Update()
	{
		base.Update();

		if ( !IsDirty )
			return;

		var indices = new List<uint>();
		for ( int i = 0; i < RectCount; ++i )
		{
			indices.AddRange( RectIndices.Select( x => (uint)(x + i * 4) ).ToArray() );
		}

		Model = new( Vertices.ToArray(), indices.ToArray(), Material );
		SetModel( Model );

		IsDirty = false;
	}
}
