using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.AssetCompiler;

public class AssetCompilerBase : IAssetCompiler
{
	protected List<BaseCompiler> Compilers = new();

	public AssetCompilerBase()
	{
		IAssetCompiler.Current = this;

		foreach ( var type in Assembly.GetExecutingAssembly().GetTypes().Where( x => x.BaseType == typeof( BaseCompiler ) ) )
		{
			var instance = Activator.CreateInstance( type ) as BaseCompiler;

			if ( instance != null )
				Compilers.Add( instance );
		}
	}

	protected bool GetCompiler( string fileExtension, [NotNullWhen( true )] out BaseCompiler? foundCompiler )
	{
		foreach ( var compiler in Compilers )
		{
			if ( compiler.GetType().GetCustomAttribute<HandlesAttribute>()?.Extensions?.Contains( fileExtension ) ?? false )
			{
				foundCompiler = compiler;
				return true;
			}
		}

		foundCompiler = null;
		return false;
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
