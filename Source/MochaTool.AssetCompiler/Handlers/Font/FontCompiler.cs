﻿using System.Diagnostics;
using System.Text.Json;

namespace MochaTool.AssetCompiler;

/// <summary>
/// A compiler for .ttf font files.
/// </summary>
[Handles( ".ttf" )]
public class FontCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Font";

	/// <inheritdoc/>
	public override string CompiledExtension => "mfnt_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	/// <inheritdoc/>
	public override string[] AssociatedFiles => s_associatedFiles;
	private static readonly string[] s_associatedFiles = new string[]
	{
		"{SourcePathWithoutExt}.txt"
	};

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		// TODO: Fix this
		if ( input.SourcePath is null )
			throw new NotSupportedException( "Compiling a font requires compiling files on disk" );

		var destJsonFileName = Path.ChangeExtension( input.SourcePath, "temp.json" )!;
		var destAtlasFileName = Path.ChangeExtension( input.SourcePath, "png" )!;

		// Create the msdf-atlas-gen process.
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				// Don't make a new window. This won't stop output, but RedirectStandardOutput causes the process to never finish
				UseShellExecute = false,
				RedirectStandardOutput = false,
				FileName = Path.GetDirectoryName( Environment.ProcessPath ) + "\\msdf-atlas-gen.exe",
				Arguments = $"-font {input.SourcePath} -imageout {destAtlasFileName} -json {destJsonFileName}"
			}
		};

		// Do we have a character set? If so, use it
		if ( input.AssociatedData.ContainsKey( "{SourcePathWithoutExt}.txt" ) )
			process.StartInfo.Arguments += $" -charset {Path.ChangeExtension( input.SourcePath, "txt" )}";

		process.Start();
		process.WaitForExit();

		// Ensure that the process succeeded. Cleanup and throw if not.
		if ( process.ExitCode != 0 )
		{
			// Delete temporary files.
			FileSystem.Mounted.Delete( destJsonFileName, FileSystemOptions.AssetCompiler );
			FileSystem.Mounted.Delete( destAtlasFileName, FileSystemOptions.AssetCompiler );

			throw new Exception( $"There was an error generating the atlas: {process.ExitCode}" );
		}

		// Write atlas metadata
		var textureMeta = new TextureMetadata()
		{
			Format = TextureFormat.RGBA
		};
		FileSystem.Mounted.WriteAllText( Path.ChangeExtension( input.SourcePath, "meta" )!, JsonSerializer.Serialize( textureMeta ), FileSystemOptions.AssetCompiler );

		// Compile atlas
		IAssetCompiler.Current!.CompileFile( destAtlasFileName );

		// Load json
		var fileData = FileSystem.Mounted.ReadAllText( destJsonFileName, FileSystemOptions.AssetCompiler );
		var fontData = JsonSerializer.Deserialize<Font.Data>( fileData );
		fontData!.ModifiedDate = FileSystem.Mounted.GetLastWriteTime( input.SourcePath, FileSystemOptions.AssetCompiler ).ToString();

		// Wrapper for file
		var mochaFile = new MochaFile<Font.Data>()
		{
			MajorVersion = 4,
			MinorVersion = 0,
			Data = fontData,
			AssetHash = input.DataHash
		};

		// Delete temporary files.
		FileSystem.Mounted.Delete( destJsonFileName, FileSystemOptions.AssetCompiler );
		FileSystem.Mounted.Delete( destAtlasFileName, FileSystemOptions.AssetCompiler );

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}
}
