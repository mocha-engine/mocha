namespace Mocha.Common;

public class TaskPool<T>
{
	public delegate Task TaskCallback( List<T> taskQueue );
	public delegate void Continuation();

	private List<Task> _tasks = new List<Task>();
	private Continuation? _continuation;

	private TaskPool( List<T> queue, TaskCallback taskStart )
	{
		var maxTasks = (int)(Environment.ProcessorCount * 0.5) - 1;
		var batchSize = queue.Count / maxTasks;

		if ( batchSize == 0 )
			batchSize = 1;

		var batched = queue
			.Select( ( value, index ) => new
			{
				Value = value,
				Index = index
			} )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToList() )
			.ToList();

		for ( int i = 0; i < batched.Count; i++ )
		{
			var taskQueue = batched[i];

			_tasks.Add( Task.Run( () => taskStart( taskQueue ) ) );
		}
	}

	public static TaskPool<T> Dispatch( List<T> queue, TaskCallback taskStart )
	{
		return new TaskPool<T>( queue, taskStart );
	}

	public TaskPool<T> Then( Continuation continuation )
	{
		_continuation = continuation;
		Task.WhenAll( _tasks ).ContinueWith( t => _continuation() ).Wait();
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
