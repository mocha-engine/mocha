using Mocha.Common.Serialization;
using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public class Texture : Asset
{
	public int Width { get; set; }
	public int Height { get; set; }

	public Veldrid.Texture VeldridTexture { get; }
	public Veldrid.TextureView VeldridTextureView { get; }

	public Vector2 Size => new Vector2( Width, Height );

	internal Texture( string path, Veldrid.Texture texture, Veldrid.TextureView textureView, int width, int height )
	{
		Path = path;
		VeldridTexture = texture;
		VeldridTextureView = textureView;
		Width = width;
		Height = height;

		All.Add( this );
	}

	public Texture( string path )
	{
		var fileBytes = FileSystem.Game.ReadAllBytes( path );
		var textureFormat = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes );

		var width = textureFormat.Data.Width;
		var height = textureFormat.Data.Height;
		var data = textureFormat.Data.MipData;
		var compressionFormat = textureFormat.Data.CompressionFormat;
		var mipCount = textureFormat.Data.MipCount;

		var textureDescription = TextureDescription.Texture2D(
			width,
			height,
			(uint)mipCount,
			1,
			compressionFormat,
			TextureUsage.Sampled
		);

		var texture = Device.ResourceFactory.CreateTexture( textureDescription );

		for ( int i = 0; i < mipCount; i++ )
		{
			int mip = i;

			var mipData = data[mip];
			var mipDataPtr = Marshal.AllocHGlobal( mipData.Length );

			int mipWidth = MathX.CalcMipSize( (int)width, mip );
			int mipHeight = MathX.CalcMipSize( (int)height, mip );

			Marshal.Copy( mipData, 0, mipDataPtr, mipData.Length );
			Device.UpdateTexture( texture,
						mipDataPtr,
						(uint)mipData.Length,
						0,
						0,
						0,
						(uint)mipWidth,
						(uint)mipHeight,
						1,
						(uint)i,
						0 );
			Marshal.FreeHGlobal( mipDataPtr );
		}

		var textureView = Device.ResourceFactory.CreateTextureView( texture );

		this.VeldridTexture = texture;
		this.VeldridTextureView = textureView;
		this.Width = (int)width;
		this.Height = (int)height;
		this.Path = path;
	}

	public void Delete()
	{
		Asset.All.Remove( this );

		VeldridTexture.Dispose();
		VeldridTextureView.Dispose();
	}

	public void Update( byte[] data, int x, int y, int width, int height )
	{
		Device.UpdateTexture( VeldridTexture, data, (uint)x, (uint)y, 0, (uint)width, (uint)height, 1, 0, 0 );
	}
}
