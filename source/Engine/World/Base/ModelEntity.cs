namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
	public Vector3 Velocity
	{
		get => Glue.Entities.GetVelocity( NativeHandle );
		set => Glue.Entities.SetVelocity( NativeHandle, value );
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
