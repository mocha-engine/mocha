using Mocha;
using Mocha.Glue;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MochaTool.AssetCompiler;

partial class ShaderCompiler
{
	[StructLayout( LayoutKind.Sequential )]
	struct DefaultAttributeData
	{
		public float ValueR { get; set; }
		public float ValueG { get; set; }
		public float ValueB { get; set; }
		public float ValueA { get; set; }
	};

	[StructLayout( LayoutKind.Sequential )]
	struct NativeShaderReflectionAttribute
	{
		public ShaderReflectionAttributeType Type { get; set; }
		public IntPtr Data { get; set; }

		internal ShaderReflectionAttribute ToManaged()
		{
			var ret = new ShaderReflectionAttribute()
			{
				Type = Type
			};

			if ( Data != IntPtr.Zero )
			{
				switch ( Type )
				{
					case ShaderReflectionAttributeType.Default:
						var d = Marshal.PtrToStructure<DefaultAttributeData>( Data );
						ret.SetData( d );
						break;

					// No data
					case ShaderReflectionAttributeType.Unknown:
					case ShaderReflectionAttributeType.SrgbRead:
						break;
				}
			}

			return ret;
		}
	};

	[StructLayout( LayoutKind.Sequential )]
	struct NativeShaderReflectionBinding
	{
		public int Set { get; set; }
		public int Binding { get; set; }
		public ShaderReflectionType Type { get; set; }
		public string Name { get; set; }

		public UtilArray Attributes { get; set; }

		internal ShaderReflectionBinding ToManaged()
		{
			var attributes = new ShaderReflectionAttribute[Attributes.count];
			for ( int i = 0; i < Attributes.count; i++ )
			{
				var native = Marshal.PtrToStructure<NativeShaderReflectionAttribute>( Attributes.data + (i * Marshal.SizeOf<NativeShaderReflectionAttribute>()) );
				attributes[i] = native.ToManaged();
			}

			return new ShaderReflectionBinding()
			{
				Set = Set,
				Binding = Binding,
				Type = Type,
				Name = new string( Name ),

				Attributes = attributes
			};
		}
	}

	struct NativeShaderReflectionInfo
	{
		public UtilArray Bindings { get; set; }

		internal ShaderReflectionInfo ToManaged()
		{
			var bindings = new ShaderReflectionBinding[Bindings.count];
			for ( int i = 0; i < Bindings.count; i++ )
			{
				var native = Marshal.PtrToStructure<NativeShaderReflectionBinding>( Bindings.data + (i * Marshal.SizeOf<NativeShaderReflectionBinding>()) );
				bindings[i] = native.ToManaged();
			}

			return new ShaderReflectionInfo()
			{
				Bindings = bindings
			};
		}
	}

	struct NativeShaderStageInfo
	{
		public int[] Data { get; set; }
		public NativeShaderReflectionInfo Reflection { get; set; }

		internal ShaderStageInfo ToManaged()
		{
			return new ShaderStageInfo()
			{
				Data = Data,
				Reflection = Reflection.ToManaged()
			};
		}
	}

	struct NativeShaderInfo
	{
		public NativeShaderStageInfo Vertex { get; set; }
		public NativeShaderStageInfo Fragment { get; set; }
		public NativeShaderStageInfo Compute { get; set; }

		internal ShaderInfo ToManaged()
		{
			return new ShaderInfo()
			{
				Compute = Compute.ToManaged(),
				Fragment = Fragment.ToManaged(),
				Vertex = Vertex.ToManaged()
			};
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	private struct NativeShaderCompilerResult
	{
		public UtilArray ShaderData;
		public NativeShaderReflectionInfo ReflectionData;
	}

	[DllImport( "MochaTool.ShaderCompilerBindings.dll", CharSet = CharSet.Ansi )]
	private static extern NativeShaderCompilerResult CompileShader( ShaderType shaderType, string shaderSource );
}
