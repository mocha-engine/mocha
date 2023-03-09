namespace MochaTool.InteropGen;

public class ThreadDispatcher<T>
{
	public delegate void ThreadCallback( List<T> threadQueue );
	public delegate Task AsyncThreadCallback( List<T> threadQueue );

	public bool IsComplete => _threadsCompleted >= _threadCount;

	private int _threadCount = (int)Math.Ceiling( Environment.ProcessorCount * 0.75 );
	private int _threadsCompleted = 0;

	public ThreadDispatcher( ThreadCallback threadStart, List<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ) );
	}

	public ThreadDispatcher( AsyncThreadCallback threadStart, List<T> queue )
	{
		Setup( queue, threadQueue => threadStart( threadQueue ).Wait() );
	}

	private void Setup( List<T> queue, Action<List<T>> threadStart )
	{
		var batchSize = queue.Count / _threadCount - 1;

		if ( batchSize == 0 )
			return; // Bail to avoid division by zero

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
