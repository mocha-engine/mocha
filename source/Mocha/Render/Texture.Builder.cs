﻿using StbImageSharp;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using Veldrid;

namespace Mocha;

public partial class TextureBuilder
{
	private string type = "texture_diffuse";

	private byte[] data;
	private uint width;
	private uint height;

	private string path;

	private bool shouldGenerateMips = false;
	private bool isCompressed = false;

	public TextureBuilder()
	{
		path = GetHashCode().ToString();
	}

	public static TextureBuilder Default => new TextureBuilder();
	public static TextureBuilder WorldTexture => new TextureBuilder().GenerateMips();
	public static TextureBuilder UITexture => new TextureBuilder();

	private static bool TryGetExistingTexture( string path, out Texture texture )
	{
		var existingTexture = Asset.All.OfType<Texture>().ToList().FirstOrDefault( t => t.Path == path );
		if ( existingTexture != null )
		{
			texture = existingTexture;
			return true;
		}

		texture = default;
		return false;
	}

	public Texture Build()
	{
		if ( TryGetExistingTexture( path, out var existingTexture ) )
			return existingTexture;

		uint mipLevels = 1;

		if ( isCompressed )
			shouldGenerateMips = false;

		if ( shouldGenerateMips )
			mipLevels = 3;

		var textureDescription = TextureDescription.Texture2D(
			width,
			height,
			mipLevels,
			1,
			isCompressed ? PixelFormat.BC3_UNorm : PixelFormat.R8_G8_B8_A8_UNorm,
			TextureUsage.Sampled //| TextureUsage.GenerateMipmaps
		);

		var texture = Device.ResourceFactory.CreateTexture( textureDescription );

		var textureDataPtr = Marshal.AllocHGlobal( data.Length );
		Marshal.Copy( data, 0, textureDataPtr, data.Length );
		Device.UpdateTexture( texture, textureDataPtr, (uint)data.Length, 0, 0, 0, width, height, 1, 0, 0 );
		Marshal.FreeHGlobal( textureDataPtr );

		var textureView = Device.ResourceFactory.CreateTextureView( texture );

		return new Texture( path, texture, textureView, type, (int)width, (int)height )
		{
			IsDirty = shouldGenerateMips
		};
	}

	public TextureBuilder WithType( string type = "texture_diffuse" )
	{
		this.type = type;

		return this;
	}

	public TextureBuilder GenerateMips( bool generateMips = true )
	{
		if ( !generateMips )
			return this;

		this.shouldGenerateMips = true;
		return this;
	}

	public TextureBuilder FromMochaTexture( string path )
	{
		if ( TryGetExistingTexture( path, out _ ) )
			return new TextureBuilder() { path = path };

		var fileBytes = File.ReadAllBytes( path );

		var textureFormat = Serializer.Deserialize<TextureInfo>( fileBytes );
		this.width = textureFormat.Data.Width;
		this.height = textureFormat.Data.Height;
		this.data = textureFormat.Data.Data;
		this.path = path;

		this.isCompressed = true;

		return this;
	}

	public TextureBuilder FromPath( string path, bool flipY = false )
	{
		if ( TryGetExistingTexture( path, out _ ) )
			return new TextureBuilder() { path = path };

		var fileData = File.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		this.data = image.Data;
		this.width = (uint)image.Width;
		this.height = (uint)image.Height;
		this.path = path;

		return this;
	}

	public TextureBuilder FromData( byte[] data, uint width, uint height )
	{
		this.data = data;
		this.width = width;
		this.height = height;

		return this;
	}

	public TextureBuilder FromStream( Stream stream, bool flipY = true )
	{
		// shit-tier hack
		if ( flipY )
			StbImage.stbi_set_flip_vertically_on_load( 1 );

		var fileData = new byte[stream.Length];
		stream.Read( fileData, 0, fileData.Length );

		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		StbImage.stbi_set_flip_vertically_on_load( 0 );

		this.data = image.Data;
		this.width = (uint)image.Width;
		this.height = (uint)image.Height;
		this.path = $"Stream {stream.GetHashCode()}";

		return this;
	}

	public TextureBuilder WithName( string name )
	{
		this.path = name;

		return this;
	}

	public TextureBuilder FromColor( Vector4 vector4 )
	{
		var data = new byte[4];

		data[0] = (byte)(vector4.X * 255).FloorToInt();
		data[1] = (byte)(vector4.Y * 255).FloorToInt();
		data[2] = (byte)(vector4.Z * 255).FloorToInt();
		data[3] = (byte)(vector4.W * 255).FloorToInt();

		return FromData( data, 1, 1 );
	}

	public TextureBuilder FromInternal( string name )
	{
		this.path = name;

		return this;
	}
}
