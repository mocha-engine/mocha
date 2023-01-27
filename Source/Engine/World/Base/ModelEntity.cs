using System.Runtime.InteropServices;

namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
	[Category( "Physics" )]
	public Vector3 Velocity
	{
		get => Glue.Entities.GetVelocity( NativeHandle );
		set => Glue.Entities.SetVelocity( NativeHandle, value );
	}

	[Category( "Physics" )]
	public float Mass
	{
		get => Glue.Entities.GetMass( NativeHandle );
		set => Glue.Entities.SetMass( NativeHandle, value );
	}

	[Category( "Physics" )]
	public float Friction
	{
		get => Glue.Entities.GetFriction( NativeHandle );
		set => Glue.Entities.SetFriction( NativeHandle, value );
	}

	[Category( "Physics" )]
	public float Restitution
	{
		get => Glue.Entities.GetRestitution( NativeHandle );
		set => Glue.Entities.SetRestitution( NativeHandle, value );
	}

	[Category( "Physics" )]
	public bool IgnoreRigidbodyRotation
	{
		get => Glue.Entities.GetIgnoreRigidbodyRotation( NativeHandle );
		set => Glue.Entities.SetIgnoreRigidbodyRotation( NativeHandle, value );
	}

	[Category( "Physics" )]
	public bool IgnoreRigidbodyPosition
	{
		get => Glue.Entities.GetIgnoreRigidbodyPosition( NativeHandle );
		set => Glue.Entities.SetIgnoreRigidbodyPosition( NativeHandle, value );
	}

	[Category( "Rendering" )]
	public IModel Model
	{
		set => Glue.Entities.SetModel( NativeHandle, value.NativeModel );
	}

	public ModelEntity()
	{
	}

	public ModelEntity( string path )
	{
		Model = new Model( path );
	}

	protected override void CreateNativeEntity()
	{
		NativeHandle = Glue.Entities.CreateModelEntity();
	}

	public void SetCubePhysics( Vector3 bounds, bool isStatic )
	{
		Glue.Entities.SetCubePhysics( NativeHandle, bounds, isStatic );
	}

	public void SetSpherePhysics( float radius, bool isStatic )
	{
		Glue.Entities.SetSpherePhysics( NativeHandle, radius, isStatic );
	}

	// TODO: Replace...
	public void SetMeshPhysics( string path )
	{
		using var _ = new Stopwatch( "Mocha phys model generation" );
		var fileBytes = FileSystem.Mounted.ReadAllBytes( path );
		var modelFile = Serializer.Deserialize<MochaFile<byte[]>>( fileBytes );

		using var stream = new MemoryStream( modelFile.Data );
		using var binaryReader = new BinaryReader( stream );

		var vertexList = new List<Vector3>();

		binaryReader.ReadChars( 4 ); // MMSH

		var verMajor = binaryReader.ReadInt32();
		var verMinor = binaryReader.ReadInt32();

		Log.Trace( $"Mocha model {verMajor}.{verMinor}" );

		binaryReader.ReadInt32(); // Pad

		var meshCount = binaryReader.ReadInt32();

		Log.Trace( $"{meshCount} meshes" );

		for ( int i = 0; i < meshCount; i++ )
		{
			binaryReader.ReadChars( 4 ); // MTRL

			var materialPath = binaryReader.ReadString();

			binaryReader.ReadChars( 4 ); // VRTX

			var vertexCount = binaryReader.ReadInt32();
			var vertices = new List<Vertex>();

			for ( int j = 0; j < vertexCount; j++ )
			{
				var vertex = new Vertex();

				Vector3 ReadVector3()
				{
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					float z = binaryReader.ReadSingle();
					return new Vector3( x, y, z );
				}

				Vector2 ReadVector2()
				{
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					return new Vector2( x, y );
				}

				vertex.Position = ReadVector3();
				vertex.Normal = ReadVector3();
				vertex.UV = ReadVector2();
				vertex.Tangent = ReadVector3();
				vertex.Bitangent = ReadVector3();

				vertices.Add( vertex );
			}

			binaryReader.ReadChars( 4 ); // INDX

			var indexCount = binaryReader.ReadInt32();
			var indices = new List<uint>();

			for ( int j = 0; j < indexCount; j++ )
			{
				indices.Add( binaryReader.ReadUInt32() );
			}

			foreach ( uint index in indices )
			{
				vertexList.Add( vertices[(int)index].Position );
			}
		}

		//
		// Reverse winding order
		//
		// vertexList.Reverse();

		unsafe
		{
			int vertexStride = Marshal.SizeOf( typeof( Vector3 ) );
			int vertexSize = vertexStride * vertexList.Count;

			fixed ( void* vertexData = vertexList.ToArray() )
			{
				Glue.Entities.SetMeshPhysics( NativeHandle, vertexSize, (IntPtr)vertexData );
			}
		}
	}
}
