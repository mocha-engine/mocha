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

	public void SetModel( Model model )
	{
		Glue.Entities.SetModel( NativeHandle, model.NativeModel.NativePtr );
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
}
