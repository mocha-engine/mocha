using Mocha.Glue;
using System.Runtime.InteropServices;

namespace Mocha.Common;

public enum ShaderReflectionType
{
	Unknown,

	Buffer,
	Texture,
	Sampler
}

[StructLayout( LayoutKind.Sequential )]
public struct ShaderReflectionBinding
{
	public int Set { get; set; }
	public int Binding { get; set; }
	public ShaderReflectionType Type { get; set; }
	public string Name { get; set; }
}

public struct ShaderReflectionInfo
{
	public UtilArray Bindings { get; set; }
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
