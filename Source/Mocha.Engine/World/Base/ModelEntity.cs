namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
	// This is a stop-gap solution until we have a proper physics body implementation

	public struct Physics
	{
		public string PhysicsModelPath { get; set; }
	}

	[HideInInspector]
	private Glue.ModelEntity NativeModelEntity => NativeEngine.GetEntityManager().GetModelEntity( NativeHandle );

	[Category( "Physics" )]
	public Vector3 Velocity
	{
		get => NativeModelEntity.GetVelocity();
		set => NativeModelEntity.SetVelocity( value );
	}

	[Category( "Physics" )]
	public float Mass
	{
		get => NativeModelEntity.GetMass();
		set => NativeModelEntity.SetMass( value );
	}

	[Category( "Physics" )]
	public float Friction
	{
		get => NativeModelEntity.GetFriction();
		set => NativeModelEntity.SetFriction( value );
	}

	[Category( "Physics" )]
	public float Restitution
	{
		get => NativeModelEntity.GetRestitution();
		set => NativeModelEntity.SetRestitution( value );
	}

	[Category( "Physics" )]
	public bool IgnoreRigidbodyRotation
	{
		get => NativeModelEntity.GetIgnoreRigidbodyRotation();
		set => NativeModelEntity.SetIgnoreRigidbodyRotation( value );
	}

	[Category( "Physics" )]
	public bool IgnoreRigidbodyPosition
	{
		get => NativeModelEntity.GetIgnoreRigidbodyPosition();
		set => NativeModelEntity.SetIgnoreRigidbodyPosition( value );
	}

	private string _modelPath;

	[Category( "Rendering" )]
	public IModel Model
	{
		set
		{
			NativeModelEntity.SetModel( value.NativeModel );
			_modelPath = value.Path;
		}
	}

	[Category( "Rendering" )]
	public string ModelPath
	{
		get => _modelPath;
		set
		{
			_modelPath = value;
			Model = new Model( value );
		}
	}

	[HideInInspector]
	public Physics PhysicsSetup { get; set; }

	public ModelEntity()
	{
	}

	public ModelEntity( string path )
	{
		Model = new Model( path );
	}

	protected override void CreateNativeEntity()
	{
		NativeHandle = NativeEngine.CreateModelEntity();
	}

	public void SetCubePhysics( Vector3 bounds, bool isStatic )
	{
		// TODO: Predicted physics
		if ( !Core.IsServer )
			return;

		NativeModelEntity.SetCubePhysics( bounds, isStatic );
	}

	public void SetSpherePhysics( float radius, bool isStatic )
	{
		// TODO: Predicted physics
		if ( !Core.IsServer )
			return;

		NativeModelEntity.SetSpherePhysics( radius, isStatic );
	}

	// TODO: Replace...
	public void SetMeshPhysics( string path )
	{
		PhysicsSetup = new Physics()
		{
			PhysicsModelPath = path
		};

		Log.Info( $"SetMeshPhysics: {path}" );

		using var _ = new Stopwatch( "Mocha phys model generation" );
		var fileBytes = FileSystem.Mounted.ReadAllBytes( path );
		var modelFile = Serializer.Deserialize<MochaFile<byte[]>>( fileBytes );

		using var stream = new MemoryStream( modelFile.Data );
		using var binaryReader = new BinaryReader( stream );

		var vertexList = new List<Vector3>();

		binaryReader.ReadChars( 4 ); // MMSH

		var verMajor = binaryReader.ReadInt32();
		var verMinor = binaryReader.ReadInt32();

		binaryReader.ReadInt32(); // Pad

		var meshCount = binaryReader.ReadInt32();

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
		NativeModelEntity.SetMeshPhysics( vertexList.ToInterop() );
	}
}
