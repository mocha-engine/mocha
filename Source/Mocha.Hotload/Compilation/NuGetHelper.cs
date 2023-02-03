using Microsoft.CodeAnalysis;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Mocha.Hotload;

/// <summary>
/// A collection of helper methods for the NuGet.Protocol package.
/// </summary>
internal static class NuGetHelper
{
	/// <summary>
	/// Fetches a NuGet package DLL and adds it to the build references.
	/// </summary>
	/// <param name="id">The ID of the NuGet package.</param>
	/// <param name="version">The version of the NuGet package.</param>
	/// <param name="references">The references to append the NuGet package to.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	internal static async Task FetchPackage( string id, NuGetVersion version, ICollection<PortableExecutableReference> references )
	{
		// Setup.
		var logger = NullLogger.Instance;
		var cancellationToken = CancellationToken.None;

		var cache = new SourceCacheContext();
		var repository = Repository.Factory.GetCoreV3( "https://api.nuget.org/v3/index.json" );
		var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

		using var packageStream = new MemoryStream();

		// Get NuGet package.
		await resource.CopyNupkgToStreamAsync(
			id,
			version,
			packageStream,
			cache,
			logger,
			cancellationToken );

		using var packageReader = new PackageArchiveReader( packageStream );
		var nuspecReader = await packageReader.GetNuspecReaderAsync( cancellationToken );

		// Find the framework target we want.
		var currentFramework = NuGetFramework.ParseFrameworkName( Compiler.GetTargetFrameworkName(), DefaultFrameworkNameProvider.Instance );
		var targetFrameworkGroup = NuGetFrameworkExtensions.GetNearest( packageReader.GetLibItems(), currentFramework );
		var dependencies = nuspecReader.GetDependencyGroups().First( group => group.TargetFramework == targetFrameworkGroup.TargetFramework ).Packages.ToArray();

		// Add dependencies.
		if ( dependencies.Length > 0 )
		{
			foreach ( var dependency in dependencies )
				await FetchPackageWithVersionRange( dependency.Id, dependency.VersionRange, references );
		}

		if ( !targetFrameworkGroup.Items.Any() )
			return;

		var dllFile = targetFrameworkGroup.Items.FirstOrDefault( item => item.EndsWith( "dll" ) );
		if ( dllFile is null )
			return;

		// Extract the correct DLL and add it to references.
		packageReader.ExtractFile( dllFile, Path.Combine( Directory.GetCurrentDirectory(), $"build\\{id}.dll" ), logger );
		references.Add( Compiler.CreateMetadataReferenceFromPath( $"build\\{id}.dll" ) );
	}

	/// <summary>
	/// Fetches all versions of a NuGet package fetches the version that best fits.
	/// </summary>
	/// <param name="id">The ID of the NuGet package.</param>
	/// <param name="versionRange">The range of versions to look at.</param>
	/// <param name="references">The references to append the NuGet package to.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	internal static async Task FetchPackageWithVersionRange( string id, VersionRange versionRange, ICollection<PortableExecutableReference> references )
	{
		// Setup.
		var cache = new SourceCacheContext();
		var repository = Repository.Factory.GetCoreV3( "https://api.nuget.org/v3/index.json" );
		var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

		// Get all versions of the package.
		var versions = await resource.GetAllVersionsAsync(
			id,
			cache,
			NullLogger.Instance,
			CancellationToken.None
			);

		// Find the best version and get it.
		var bestVersion = versionRange.FindBestMatch( versions );
		await FetchPackage( id, bestVersion, references );
	}
}
