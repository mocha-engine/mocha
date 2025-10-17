namespace Mocha;

partial class UIEntity
{
	public override void Update()
	{
		base.Update();

		if ( !IsDirty )
			return;

		var indices = new List<uint>();
		for ( int i = 0; i < _rectCount; ++i )
		{
			indices.AddRange( RectIndices.Select( x => (uint)(x + i * 4) ).ToArray() );
		}

		UIModel = new( _vertices.ToArray(), indices.ToArray(), Material );
		Model = UIModel;

		IsDirty = false;
	}
}
