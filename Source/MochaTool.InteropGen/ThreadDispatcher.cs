namespace MochaTool.InteropGen;

public class ThreadDispatcher<T>
{
	public delegate void ThreadCallback( List<T> threadQueue );

	private int _threadCount = 16;
	private int _threadsCompleted = 0;
	public bool IsComplete => _threadsCompleted == _threadCount;

	public ThreadDispatcher( ThreadCallback threadStart, List<T> queue )
	{
		var batchSize = queue.Count / _threadCount - 1;

		if ( batchSize == 0 )
			return; // Bail to avoid division by zero

		var batched = queue
			.Select( ( Value, Index ) => new { Value, Index } )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToList() )
			.ToList();

		if ( batched.Count < _threadCount )
			_threadCount = batched.Count; // Min. 1 per thread

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
