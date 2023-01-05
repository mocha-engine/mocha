using Mocha.Common.Serialization;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".ttf" } )]
public class FontCompiler : BaseCompiler
{
	public override string CompileFile( string path )
	{
		Log.Processing( "Font", path );

		var destJsonFileName = Path.ChangeExtension( path, "temp.json" );
		var destAtlasFileName = Path.ChangeExtension( path, "png" );
		var destAtlasMeta = Path.ChangeExtension( path, "meta" );
		var destFileName = Path.ChangeExtension( path, "mfnt_c" );

		// TODO: Move to a nice generic function somewhere
		if ( File.Exists( destFileName ) )
		{
			// Read mocha file
			var existingFile = File.ReadAllBytes( destFileName );
			var deserializedFile = Serializer.Deserialize<MochaFile<Font.Data>>( existingFile );

			if ( deserializedFile.Data.ModifiedDate == File.GetLastWriteTime( path ).ToString() )
			{
				Log.Skip( path );
				return destFileName;
			}
		}

		var charSetFileName = Path.ChangeExtension( path, ".txt" );

		// Get exe directory
		var exeDir = Path.GetDirectoryName( Process.GetCurrentProcess().MainModule.FileName );
		string exePath = exeDir + "\\msdf-atlas-gen.exe";
		string fontPath = path;

		// Create a new ProcessStartInfo object
		ProcessStartInfo startInfo = new ProcessStartInfo();

		// Don't output
		startInfo.UseShellExecute = true;
		startInfo.FileName = exePath;
		startInfo.Arguments = $"-font {fontPath} -imageout {destAtlasFileName} -json {destJsonFileName}";

		// Do we have a character set? If so, use it
		if ( File.Exists( charSetFileName ) )
			startInfo.Arguments += $" -charset {charSetFileName}";

		// Create a new Process object and start it
		Process process = new Process();
		process.StartInfo = startInfo;
		process.Start();

		// Wait for the process to finish
		process.WaitForExit();

		if ( process.ExitCode != 0 )
		{
			// There was an error generating the atlas
			throw new Exception();
		}

		// Write atlas metadata
		var textureMeta = new TextureMetadata()
		{
			Format = TextureFormat.R8G8B8A8_SRGB
		};

		File.WriteAllText( destAtlasMeta, JsonSerializer.Serialize( textureMeta ) );

		// Compile atlas
		OfflineCompiler.Current.CompileFile( destAtlasFileName );

		// Load json
		var fileData = File.ReadAllText( destJsonFileName );
		var fontData = JsonSerializer.Deserialize<Font.Data>( fileData );

		fontData.ModifiedDate = File.GetLastWriteTime( path ).ToString();

		// Wrapper for file
		var mochaFile = new MochaFile<Font.Data>()
		{
			MajorVersion = 3,
			MinorVersion = 1,
			Data = fontData
		};

		// Calculate original asset hash
		using ( var md5 = MD5.Create() )
			mochaFile.AssetHash = md5.ComputeHash( Encoding.Default.GetBytes( fileData ) );

		// Write result
		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( Serializer.Serialize( mochaFile ) );

		//
		// Cleanup
		//

		// Delete temporary json file
		File.Delete( destJsonFileName );

		// Delete original atlas file
		File.Delete( destAtlasFileName );
		return destJsonFileName;
	}
}
