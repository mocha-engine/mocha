using Microsoft.Extensions.Logging;

namespace MochaTool.InteropGen.Extensions;

/// <summary>
/// Contains extension methods for <see cref="ILogger"/>s.
/// </summary>
internal static partial class ILoggerExtensions
{
	/// <summary>
	/// Logs the first message to the user.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	[LoggerMessage( EventId = 0,
		Level = LogLevel.Information,
		Message = "Generating C# <--> C++ interop code..." )]
	internal static partial void LogIntro( this ILogger logger );

	/// <summary>
	/// Logs an error about missing required args.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	[LoggerMessage( EventId = -1,
		Level = LogLevel.Error,
		Message = "The base directory to generate code from is required to run this tool" )]
	internal static partial void LogIntroError( this ILogger logger );

	/// <summary>
	/// Logs a timed operation to the user.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	/// <param name="name">The name of the timed operation.</param>
	/// <param name="seconds">The time in seconds that it took to complete the operation.</param>
	[LoggerMessage( EventId = 1,
		Message = "{name} took {seconds} seconds." )]
	internal static partial void ReportTime( this ILogger logger, LogLevel logLevel, string name, double seconds );

	/// <summary>
	/// Logs to the user that a header is being processed by the parser.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	/// <param name="path">The absolute path to the header file being processed.</param>
	[LoggerMessage( EventId = 2,
		Level = LogLevel.Debug,
		Message = "Processing header {path}..." )]
	internal static partial void ProcessingHeader( this ILogger logger, string path );

	/// <summary>
	/// Logs a fatal C++ diagnostic to the user.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	/// <param name="diagnostic">The diagnostic to show.</param>
	[LoggerMessage( EventId = 3,
		Level = LogLevel.Warning,
		Message = "{diagnostic}" )]
	internal static partial void FatalDiagnostic( this ILogger logger, string diagnostic );

	/// <summary>
	/// Logs an error C++ diagnostic to the user.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	/// <param name="diagnostic">The diagnostic to show.</param>
	[LoggerMessage( EventId = 4,
		Level = LogLevel.Warning,
		Message = "{diagnostic}" )]
	internal static partial void ErrorDiagnostic( this ILogger logger, string diagnostic );

	/// <summary>
	/// Logs a warning C++ diagnostic to the user.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger"/> instance to log to.</param>
	/// <param name="diagnostic">The diagnostic to show.</param>
	[LoggerMessage( EventId = 5,
		Level = LogLevel.Warning,
		Message = "{diagnostic}" )]
	internal static partial void WarnDiagnostic( this ILogger logger, string diagnostic );
}
