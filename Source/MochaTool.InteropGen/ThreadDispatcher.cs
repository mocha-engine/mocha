namespace MochaTool.InteropGen;

internal sealed class ThreadDispatcher<T>
{
	internal delegate void ThreadCallback( T[] threadQueue );
	internal delegate Task AsyncThreadCallback( T[] threadQueue );

	internal bool IsComplete => _threadsCompleted >= _threadCount;

	private int _threadCount = (int)Math.Ceiling( Environment.ProcessorCount * 0.75 );
	private int _threadsCompleted = 0;

	internal ThreadDispatcher( ThreadCallback threadStart, IEnumerable<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ) );
	}

	internal ThreadDispatcher( AsyncThreadCallback threadStart, IEnumerable<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ).Wait() );
	}

	private void Setup( IEnumerable<T> queue, Action<T[]> threadStart )
	{
		var batchSize = queue.Count() / (_threadCount - 1);

		if ( batchSize == 0 )
			throw new InvalidOperationException( "There are no items to batch for threads" );

		var batched = queue
			.Select( ( Value, Index ) => new { Value, Index } )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToArray() )
			.ToArray();

		_threadCount = batched.Length;

		for ( int i = 0; i < batched.Length; i++ )
		{
			var threadQueue = batched[i];
			var thread = new Thread( () =>
			{
				threadStart( threadQueue );
				Interlocked.Increment( ref _threadsCompleted );
			} );

			thread.Start();
		}
	}
}
