using System.Runtime.InteropServices;

namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
	public Vector3 Velocity
	{
		get => Glue.Entities.GetVelocity( NativeHandle );
		set => Glue.Entities.SetVelocity( NativeHandle, value );
	}

	public float Mass
	{
		get => Glue.Entities.GetMass( NativeHandle );
		set => Glue.Entities.SetMass( NativeHandle, value );
	}

	public float Friction
	{
		get => Glue.Entities.GetFriction( NativeHandle );
		set => Glue.Entities.SetFriction( NativeHandle, value );
	}

	public float Restitution
	{
		get => Glue.Entities.GetRestitution( NativeHandle );
		set => Glue.Entities.SetRestitution( NativeHandle, value );
	}

	public bool IgnoreRigidbodyRotation
	{
		get => Glue.Entities.GetIgnoreRigidbodyRotation( NativeHandle );
		set => Glue.Entities.SetIgnoreRigidbodyRotation( NativeHandle, value );
	}

	public bool IgnoreRigidbodyPosition
	{
		get => Glue.Entities.GetIgnoreRigidbodyPosition( NativeHandle );
		set => Glue.Entities.SetIgnoreRigidbodyPosition( NativeHandle, value );
	}

	public ModelEntity()
	{
	}

	public ModelEntity( string path )
	{
		SetModel( path );
	}

	protected override void CreateNativeEntity()
	{
		NativeHandle = Glue.Entities.CreateModelEntity();
	}

	public void SetModel( IModel model )
	{
		Glue.Entities.SetModel( NativeHandle, model.NativeModel );
	}

	public void SetModel( string modelPath )
	{
		var model = new Model( modelPath );
		SetModel( model );
	}

	public void SetCubePhysics( Vector3 bounds, bool isStatic )
	{
		Glue.Entities.SetCubePhysics( NativeHandle, bounds, isStatic );
	}

	public void SetSpherePhysics( float radius, bool isStatic )
	{
		Glue.Entities.SetSpherePhysics( NativeHandle, radius, isStatic );
	}

	// TODO: SHIT!!
	public void SetMeshPhysics( string path )
	{
		using var _ = new Stopwatch( "Mocha phys model generation" );
		using var fileStream = FileSystem.Game.OpenRead( path );
		using var binaryReader = new BinaryReader( fileStream );

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
					// binaryReader.ReadInt32();
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					float z = binaryReader.ReadSingle();
					return new Vector3( x, y, z );
				}

				Vector2 ReadVector2()
				{
					// binaryReader.ReadInt32();
					// binaryReader.ReadInt32();
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					return new Vector2( x, y );
				}

				vertex.Position = ReadVector3();
				vertex.Normal = ReadVector3();
				vertex.UV = ReadVector2();
				// vertex.Tangent = ReadVector3();
				// vertex.Bitangent = ReadVector3();

				ReadVector3();
				ReadVector3();

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
