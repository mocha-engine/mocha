using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;
using StbImageSharp;
using System.Text.Json;
using TinyEXR;

namespace MochaTool.AssetCompiler;

/// <summary>
/// A compiler for .hdri, .hdr, and .exr environment maps.
/// </summary>
[Handles( ".hdri", ".hdr", ".exr" )]
public partial class EnvironmentMapCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Environment Map";

	/// <inheritdoc/>
	public override string CompiledExtension => "menv_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	/// <inheritdoc/>
	public override string[] AssociatedFiles => s_associatedFiles;
	private static readonly string[] s_associatedFiles = new string[]
	{
		"{SourcePathWithoutExt}.meta"
	};

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		var textureMeta = new TextureMetadata();

		// Check for meta, load if it exists.
		if ( input.AssociatedData.TryGetValue( "{SourcePathWithoutExt}.meta", out var metaData ) )
			textureMeta = JsonSerializer.Deserialize<TextureMetadata>( metaData.Span );

		// Setup image and format.
		TextureInfo textureFormat;
		float[] imageData;

		if ( input.SourcePath?.EndsWith( ".exr", StringComparison.CurrentCultureIgnoreCase ) ?? false )
		{
			// Use tinyexr for exr
			Exr.LoadFromMemory( input.SourceData.Span, out imageData, out var imageWidth, out var imageHeight );
			textureFormat = new TextureInfo
			{
				DataWidth = (uint)imageWidth,
				DataHeight = (uint)imageHeight,
				Width = (uint)imageWidth,
				Height = (uint)imageHeight,
				MipCount = 1,
				Format = textureMeta.Format
			};
		}
		else
		{
			// Use stb_image for hdri
			var image = ImageResultFloat.FromMemory( input.SourceData.ToArray(), ColorComponents.RedGreenBlueAlpha );
			textureFormat = new TextureInfo
			{
				DataWidth = (uint)image.Width,
				DataHeight = (uint)image.Height,
				Width = (uint)image.Width,
				Height = (uint)image.Height,
				MipCount = 1,
				Format = textureMeta.Format
			};

			imageData = image.Data;
		}

		textureFormat.MipData = new byte[textureFormat.MipCount][];
		textureFormat.MipDataLength = new int[textureFormat.MipCount];

		// Setup mip-maps.
		for ( uint i = 0; i < textureFormat.MipCount; ++i )
		{
			textureFormat.MipData[i] = BlockCompression( imageData, textureFormat.DataWidth, textureFormat.DataHeight, i );
			textureFormat.MipDataLength[i] = textureFormat.MipData[i].Length;
		}

		// Wrapper for file.
		var mochaFile = new MochaFile<TextureInfo>
		{
			MajorVersion = 4,
			MinorVersion = 0,
			Data = textureFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}

	private static ReadOnlyMemory2D<ColorRgbFloat> FloatToColorRgbFloat( float[] data, int width, int height )
	{
		var result = new ColorRgbFloat[width * height];
		for ( int i = 0; i < result.Length; i++ )
		{
			result[i] = new ColorRgbFloat( data[i * 4], data[i * 4 + 1], data[i * 4 + 2] );
		}

		return new ReadOnlyMemory2D<ColorRgbFloat>( result, width, height );
	}

	private static byte[] BlockCompression( float[] data, uint width, uint height, uint mip )
	{
		var encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = true;
		encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
		encoder.OutputOptions.Format = CompressionFormat.Bc6S;
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		var input = FloatToColorRgbFloat( data, (int)width, (int)height );

		return encoder.EncodeToRawBytesHdr( input, (int)mip, out _, out _ );
	}
}
