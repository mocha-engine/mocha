using System.Runtime.InteropServices;

namespace Mocha.Common;

public enum ShaderReflectionType
{
	Unknown,

	Buffer,
	Texture,
	Sampler
}

public enum ShaderReflectionAttributeType
{
	Unknown,

	// SrgbReadAttribute
	SrgbRead,

	// DefaultAttribute
	Default
};

[StructLayout( LayoutKind.Sequential )]
public struct DefaultAttributeData
{
	public float ValueR { get; set; }
	public float ValueG { get; set; }
	public float ValueB { get; set; }
	public float ValueA { get; set; }
};

[StructLayout( LayoutKind.Sequential )]
public struct ShaderReflectionAttribute
{
	public ShaderReflectionAttributeType Type { get; set; }
	public byte[] Data { get; set; }

	public void SetData<T>( T? obj ) where T : new()
	{
		if ( obj == null )
			return;

		Data = Serializer.Serialize<T>( obj );
	}

	public readonly T? GetData<T>() where T : new()
	{
		return Serializer.Deserialize<T>( Data );
	}
};

[StructLayout( LayoutKind.Sequential )]
public struct ShaderReflectionBinding
{
	public int Set { get; set; }
	public int Binding { get; set; }
	public ShaderReflectionType Type { get; set; }
	public string Name { get; set; }

	public ShaderReflectionAttribute[] Attributes { get; set; }
}

public struct ShaderReflectionInfo
{
	public ShaderReflectionBinding[] Bindings { get; set; }
}

public struct NativeShaderStageInfo
{
	public int[] Data { get; set; }
	public ShaderReflectionInfo Reflection { get; set; }
}

public struct ShaderStageInfo
{
	public int[] Data { get; set; }
	public ShaderReflectionInfo Reflection { get; set; }
}

public struct ShaderInfo
{
	public ShaderStageInfo Vertex { get; set; }
	public ShaderStageInfo Fragment { get; set; }
	public ShaderStageInfo Compute { get; set; }
}
