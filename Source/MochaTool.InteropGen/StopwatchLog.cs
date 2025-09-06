using Microsoft.Extensions.Logging;
using MochaTool.InteropGen.Extensions;
using System.Diagnostics;

namespace MochaTool.InteropGen;

/// <summary>
/// Represents a scoped logger for recording time taken to complete an operation.
/// </summary>
internal readonly struct StopwatchLog : IDisposable
{
	/// <summary>
	/// The name of the operation being completed.
	/// </summary>
	private readonly string name = "Unknown";
	/// <summary>
	/// The level at which to log the operation.
	/// </summary>
	private readonly LogLevel logLevel = LogLevel.Debug;
	/// <summary>
	/// The timestamp at which the operation started.
	/// </summary>
	private readonly long startTimestamp = Stopwatch.GetTimestamp();

	/// <summary>
	/// Initializes a new instance of <see cref="StopwatchLog"/>.
	/// </summary>
	/// <param name="name">The name of the operation being completed.</param>
	/// <param name="logLevel">The level at which to log the operation.</param>
	/// <param name="startTimestamp">The timestamp at which the operation started.</param>
	internal StopwatchLog( string name, LogLevel logLevel = LogLevel.Debug, long? startTimestamp = null )
	{
		this.name = name;
		this.logLevel = logLevel;
		this.startTimestamp = startTimestamp ?? Stopwatch.GetTimestamp();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Log the time taken to complete the operation.
		Log.ReportTime( logLevel, name, Stopwatch.GetElapsedTime( startTimestamp ).TotalSeconds );
	}
}
