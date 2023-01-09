using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.AssetCompiler;

public class AssetCompilerBase : IAssetCompiler
{
	protected List<BaseCompiler> Compilers = new();

	private readonly Dictionary<string, BaseCompiler> ExtensionToCompilerCache = new();

	public AssetCompilerBase()
	{
		IAssetCompiler.Current = this;

		// Fetch all compilers and cache them.
		foreach ( var type in Assembly.GetExecutingAssembly().GetTypes().Where( x => x.BaseType == typeof( BaseCompiler ) ) )
		{
			if ( Activator.CreateInstance( type ) is not BaseCompiler instance )
				continue;

			Compilers.Add( instance );
			var handleAttribute = instance.GetType().GetCustomAttribute<HandlesAttribute>();
			if ( handleAttribute is null )
				continue;

			foreach ( var extension in handleAttribute.Extensions )
				ExtensionToCompilerCache.Add( extension, instance );
		}
	}

	/// <summary>
	/// Attempts to get a compiler that can handle the provided file extension.
	/// </summary>
	/// <param name="fileExtension">The file extension to look for a compiler with.</param>
	/// <param name="foundCompiler">The compiler that was found. Null if none found.</param>
	/// <returns>Whether or not a compiler was found.</returns>
	protected bool GetCompiler( string fileExtension, [NotNullWhen( true )] out BaseCompiler? foundCompiler )
	{
		return ExtensionToCompilerCache.TryGetValue( fileExtension, out foundCompiler );
	}

	}

	public void CompileFile( string path )
	{
		var fileExtension = Path.GetExtension( path );

		// TODO: Check if we have an original asset & if it needs recompiling

		if ( !GetCompiler( fileExtension, out var compiler ) )
			return;

		Log.Processing( compiler.AssetName, path );
		var result = compiler.CompileFile( path );

		switch ( result.State )
		{
			case CompileState.UpToDate:
				Log.UpToDate( result.DestinationPath! );
				break;
			case CompileState.Succeeded:
				Log.Compiled( result.DestinationPath! );
				break;
			case CompileState.Failed:
				throw new Exception( "Failed to compile?" );
			default:
				throw new UnreachableException();
		}
	}
}
