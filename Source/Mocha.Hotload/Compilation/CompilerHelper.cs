using Mocha.Common;
using System.Reflection;
using System.Runtime.Versioning;

namespace Mocha.Hotload.Compilation;

/// <summary>
/// A collection of helper members for C# runtime projects and compilation.
/// </summary>
internal static class CompilerHelper
{
	/// <summary>
	/// Defines the way the process was compiled. Used for parsing which csproj items to select in projects.
	/// </summary>
#if DEBUG
	internal const string Build = "Debug";
#else
	internal const string Build = "Release";
#endif

	/// <summary>
	/// Returns the realm the runtime is operating in.
	/// NOTE: You should only access this on the main thread.
	/// </summary>
	internal static string Realm => NativeEngine.IsDedicatedServer() || Core.IsServer ? "Server" : "Client";

	/// <summary>
	/// Returns the target framework of the application.
	/// </summary>
	/// <returns>The target framework of the application.</returns>
	internal static string GetTargetFrameworkName()
	{
		// AppContext.TargetFrameworkName will always be null since the starting process is native code.
		// Leave it here anyway in case this changes.
		if ( !string.IsNullOrEmpty( AppContext.TargetFrameworkName ) )
			return AppContext.TargetFrameworkName;

		// Fallback on the TargetFrameworkAttribute of the Hotload assembly
		if ( Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>() is not TargetFrameworkAttribute frameworkAttribute )
			return string.Empty;

		return frameworkAttribute.FrameworkName;
	}

	/// <summary>
	/// Returns the C# target framework moniker in a format that the csproj standard supports.
	/// https://learn.microsoft.com/en-us/dotnet/standard/frameworks
	/// </summary>
	/// <returns>The C# target framework moniker in a format that the csproj standard supports.</returns>
	internal static string GetCSharpProjectMoniker()
	{
		var frameworkName = GetTargetFrameworkName();
		var parts = frameworkName.Split( ',' );
		var shortName = parts[0].Replace( ".NETCoreApp", "net" );
		var version = parts[1]["Version=v".Length..];

		return shortName + version;
	}
}
