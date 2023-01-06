﻿using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Mocha.Common.Serialization;
using StbImageSharp;
using System.Security.Cryptography;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".png", ".jpg" } )]
public class TextureCompiler : BaseCompiler
{
	public override string AssetName => "Texture";

	private static byte[] BlockCompression( byte[] data, int width, int height, int mip, CompressionFormat compressionFormat )
	{
		BcEncoder encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = true;
		encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
		encoder.OutputOptions.Format = compressionFormat;
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		return encoder.EncodeToRawBytes( data, width, height, PixelFormat.Rgba32, mip, out _, out _ );
	}

	public override CompileResult CompileFile( string path )
	{
		var destFileName = Path.ChangeExtension( path, "mtex_c" );
		var textureFormat = new TextureInfo();

		// Load image
		var fileData = File.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		// TODO: Move to a nice generic function somewhere
		if ( File.Exists( destFileName ) )
		{
			// Read mocha file
			var existingFile = File.ReadAllBytes( destFileName );
			var deserializedFile = Serializer.Deserialize<MochaFile<TextureInfo>>( existingFile );

			using var md5 = MD5.Create();
			var computedHash = md5.ComputeHash( fileData );
			if ( Enumerable.SequenceEqual( deserializedFile.AssetHash, computedHash ) )
				return UpToDate( path, destFileName );
		}

		textureFormat.Width = (uint)image.Width;
		textureFormat.Height = (uint)image.Height;
		textureFormat.MipCount = 5;
		textureFormat.MipData = new byte[textureFormat.MipCount][];
		textureFormat.MipDataLength = new int[textureFormat.MipCount];

		// Change compression format based on normal map
		for ( int i = 0; i < textureFormat.MipCount; ++i )
		{
			if ( path.Contains( "normal" ) )
			{
				// Do not compress
				// textureFormat.CompressionFormat = Veldrid.PixelFormat.BC5_UNorm;
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Bc5 );
			}
			else if ( path.Contains( "font" ) )
			{
				// Do not compress
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Rgba );
			}
			else if ( path.Contains( "noise" ) )
			{
				// Do not compress
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Rgba );
			}
			else
			{
				// textureFormat.CompressionFormat = Veldrid.PixelFormat.BC3_UNorm;
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Bc3 );
			}

			textureFormat.MipDataLength[i] = textureFormat.MipData[i].Length;
		}

		// Wrapper for file
		var mochaFile = new MochaFile<TextureInfo>()
		{
			MajorVersion = 3,
			MinorVersion = 1,
			Data = textureFormat
		};

		// Calculate original asset hash
		using ( var md5 = MD5.Create() )
			mochaFile.AssetHash = md5.ComputeHash( fileData );

		// Write result
		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( Serializer.Serialize( mochaFile ) );
		return Succeeded( path, destFileName );
	}
}
