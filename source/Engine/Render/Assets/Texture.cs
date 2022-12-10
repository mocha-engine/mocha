﻿using Mocha.Common.Serialization;

namespace Mocha.Renderer;

[Icon( FontAwesome.Image ), Title( "Texture" )]
public partial class Texture : Asset
{
	public uint Width { get; set; }
	public uint Height { get; set; }
	public string Type { get; set; }

	public Glue.ManagedTexture NativeTexture { get; set; }

	public Texture( string path )
	{
		var fileBytes = FileSystem.Game.ReadAllBytes( path );

		var textureFormat = Serializer.Deserialize<MochaFile<TextureInfo>>( fileBytes );
		Width = textureFormat.Data.Width;
		Height = textureFormat.Data.Height;

		var mipData = textureFormat.Data.MipData;
		var mipCount = textureFormat.Data.MipCount;

		NativeTexture = new();

		unsafe
		{
			fixed ( void* data = mipData[0] )
			{
				NativeTexture.SetData( Width, Height, (IntPtr)data, (int)TextureFormat.BC3_SRGB );
			}
		}
	}

	public Texture( uint width, uint height, byte[] data ) : this( width, height )
	{
		unsafe
		{
			fixed ( void* dataPtr = data )
			{
				NativeTexture.SetData( Width, Height, (IntPtr)dataPtr, (int)TextureFormat.R8G8B8A8_SRGB );
			}
		}
	}

	public Texture( uint width, uint height )
	{
		Width = width;
		Height = height;

		NativeTexture = new();
	}

	internal Texture( string path, string type, int width, int height )
	{
		Path = path;
		Type = type;
		Width = (uint)width;
		Height = (uint)height;

		All.Add( this );
	}

	public void Delete()
	{
		Asset.All.Remove( this );
	}
}
