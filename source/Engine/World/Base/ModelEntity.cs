namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
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
}
