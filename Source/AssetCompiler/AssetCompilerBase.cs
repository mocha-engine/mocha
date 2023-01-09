using Mocha.Common.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;

namespace Mocha.AssetCompiler;

public class AssetCompilerBase : IAssetCompiler
{
	/// <summary>
	/// A list containing all found compilers.
	/// </summary>
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
	protected bool TryGetCompiler( string fileExtension, [NotNullWhen( true )] out BaseCompiler? foundCompiler )
	{
		return ExtensionToCompilerCache.TryGetValue( fileExtension, out foundCompiler );
	}

	/// <summary>
	/// Compiles a path pattern.
	/// </summary>
	/// <param name="sourcePath">The source path of the asset.</param>
	/// <param name="pathPattern">The path pattern to compile.</param>
	/// <returns>The compiled path.</returns>
	protected static string CompilePathPattern( string sourcePath, string pathPattern )
	{
		return pathPattern
			.Replace( "{SourcePath}", sourcePath )
			.Replace( "{SourcePathWithoutExt}", Path.Combine( Path.GetDirectoryName( sourcePath )!, Path.GetFileNameWithoutExtension( sourcePath ) ) )
			.Replace( "{SourcePathDir}", Path.GetDirectoryName( sourcePath ) );
	}

	/// <inheritdoc/>
	public void CompileFile( string path ) => CompileFileAsync( path ).Wait();

	/// <inheritdoc/>
	public async Task CompileFileAsync( string path )
	{
		// Check if we have a compiler for the file.
		var fileExtension = Path.GetExtension( path );
		if ( !TryGetCompiler( fileExtension, out var compiler ) )
			return;

		var files = new Dictionary<string, ReadOnlyMemory<byte>>();
		using var md5 = MD5.Create();

		foreach ( var filePathPattern in compiler.AssociatedFiles )
		{
			var filePath = CompilePathPattern( path, filePathPattern );
			// TODO: Support wildcard (*)

			if ( !File.Exists( filePath ) )
				continue;
			
			// Add associated file and apply it to the MD5 hash.
			var data = await File.ReadAllBytesAsync( filePath );
			files.Add( filePathPattern, data );
			md5.TransformBlock( data, 0, data.Length, data, 0 );
		}

		// Finish MD5 with the source file.
		var sourceData = await File.ReadAllBytesAsync( path );
		md5.TransformFinalBlock( sourceData, 0, sourceData.Length );

		var hash = md5.Hash!;
		var compiledPath = Path.ChangeExtension( path, compiler.CompiledExtension );

		// Check if we need to recompile.
		if ( compiler.SupportsMochaFile && File.Exists( compiledPath ) )
		{
			MochaFile<object> compiledFile = Serializer.Deserialize<MochaFile<object>>( await File.ReadAllBytesAsync( compiledPath ) );
			if ( Enumerable.SequenceEqual( hash, compiledFile.AssetHash ) )
			{
				Log.UpToDate( path );
				return;
			}
		}

		Log.Processing( compiler.AssetName, path );

		// Compile.
		var input = new CompileInput()
		{
			SourcePath = path,
			SourceData = sourceData,
			AssociatedData = files,
			DataHash = md5.Hash!
		};

		CompileResult result;
		try
		{
			result = compiler.CompileFile( ref input );
		}
		catch( Exception e )
		{
			result = new CompileResult()
			{
				State = CompileState.Failed,
				Exception = e
			};
		}

		switch ( result.State )
		{
			case CompileState.Succeeded:
				// Write compiled data.
				using ( var compiledFile = File.OpenWrite( compiledPath ) )
					await compiledFile.WriteAsync( result.Data );

				foreach ( var (compiledAssociatedPathPattern, associatedData) in result.AssociatedData )
				{
					var compiledAssociatedPath = CompilePathPattern( path, compiledAssociatedPathPattern );
					using var compiledAssociatedFile = File.OpenWrite( compiledAssociatedPath );
					await compiledAssociatedFile.WriteAsync( associatedData );
				}

				Log.Compiled( compiledPath );
				break;
			case CompileState.Failed:
				Log.Fail( path, result.Exception );
				break;
			default:
				throw new UnreachableException();
		}
	}
}
