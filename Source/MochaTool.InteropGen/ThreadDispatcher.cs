namespace MochaTool.InteropGen;

internal class ThreadDispatcher<T>
{
	internal delegate void ThreadCallback( List<T> threadQueue );
	internal delegate Task AsyncThreadCallback( List<T> threadQueue );

	internal bool IsComplete => _threadsCompleted >= _threadCount;

	private int _threadCount = (int)Math.Ceiling( Environment.ProcessorCount * 0.75 );
	private int _threadsCompleted = 0;

	internal ThreadDispatcher( ThreadCallback threadStart, List<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ) );
	}

	internal ThreadDispatcher( AsyncThreadCallback threadStart, List<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ).Wait() );
	}

	private void Setup( List<T> queue, Action<List<T>> threadStart )
	{
		var batchSize = queue.Count / (_threadCount - 1);

		if ( batchSize == 0 )
			throw new InvalidOperationException( "There are no items to batch for threads" );

		var batched = queue
			.Select( ( Value, Index ) => new { Value, Index } )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToList() )
			.ToList();

		_threadCount = batched.Count;

		for ( int i = 0; i < batched.Count; i++ )
		{
			var threadQueue = batched[i];
			var thread = new Thread( () =>
			{
				threadStart( threadQueue );
				_threadsCompleted++;
			} );

			thread.Start();
		}
	}
}
