global using static MochaTool.InteropGen.Global;
using Microsoft.Extensions.Logging;

namespace MochaTool.InteropGen;

/// <summary>
/// Contains globally used items in the project.
/// </summary>
internal static class Global
{
	/// <summary>
	/// The instance of <see cref="ILogger"/> to use when logging.
	/// </summary>
	internal static readonly ILogger Log;

	/// <summary>
	/// Initializes the <see cref="Log"/> instance.
	/// </summary>
	static Global()
	{
		using var factory = LoggerFactory.Create( builder => builder
			.AddConsole()
			.SetMinimumLevel( LogLevel.Information ) );
		Log = factory.CreateLogger( "InteropGen" );
	}
}
