namespace Mocha.Common;

public sealed class TaskPool<T>
{
	public delegate Task TaskCallback( T[] taskQueue );
	public delegate void Continuation();

	private readonly Task[] _tasks;

	private TaskPool( T[] queue, TaskCallback taskStart )
	{
		var maxTasks = (int)(Environment.ProcessorCount * 0.5) - 1;
		var batchSize = queue.Length / maxTasks;

		if ( batchSize == 0 )
			batchSize = 1;

		var batched = queue
			.Select( ( value, index ) => new
			{
				Value = value,
				Index = index
			} )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToArray() )
			.ToArray();

		_tasks = new Task[batched.Length];
		for ( int i = 0; i < batched.Length; i++ )
		{
			var taskQueue = batched[i];

			_tasks[i] = Task.Run( () => taskStart( taskQueue ) );
		}
	}

	public static TaskPool<T> Dispatch( T[] queue, TaskCallback taskStart )
	{
		return new TaskPool<T>( queue, taskStart );
	}

	public static TaskPool<T> Dispatch( IEnumerable<T> queue, TaskCallback taskStart )
	{
		return new TaskPool<T>( queue.ToArray(), taskStart );
	}

	public TaskPool<T> Then( Continuation continuation )
	{
		Task.WhenAll( _tasks ).ContinueWith( t => continuation() ).Wait();
		return this;
	}

	public async Task WaitForCompleteAsync()
	{
		await Task.WhenAll( _tasks );
	}

	public void WaitForComplete()
	{
		WaitForCompleteAsync().Wait();
	}
}
