namespace Mocha.Common;

public class ThreadDispatcher<T>
{
	public delegate void ThreadCallback( List<T> threadQueue );
	private int threadCount = 16;

	private int threadsCompleted = 0;
	public bool IsComplete => threadsCompleted == threadCount - 1;

	public ThreadDispatcher( ThreadCallback threadStart, List<T> queue )
	{
		var batchSize = queue.Count / threadCount - 1;

		if ( batchSize == 0 )
			return; // Bail to avoid division by zero

		var batched = queue
			.Select( ( Value, Index ) => new { Value, Index } )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToList() )
			.ToList();

		if ( batched.Count < threadCount )
			threadCount = batched.Count; // Min. 1 per thread

		for ( int i = 0; i < batched.Count; i++ )
		{
			var threadQueue = batched[i];
			var thread = new Thread( () =>
			{
				threadStart( threadQueue );

				threadsCompleted++;
			} );

			thread.Start();
		}
	}
}
