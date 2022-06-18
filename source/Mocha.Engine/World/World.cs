using Veldrid;

namespace Mocha.Engine;

public class World
{
	public static World Current { get; set; }

	public Camera Camera { get; set; }
	public Sun Sun { get; set; }
	public Sky Sky { get; set; }

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		SetupEntities();
	}

	private void SetupEntities()
	{
		Camera = new Camera();

		Sun = new Sun()
		{
			Position = new( 0, 10, 10 ),
			Rotation = Rotation.From( 0, 0, 0 )
		};

		Sky = new Sky
		{
			Scale = Vector3.One * -100f
		};

		_ = new GenericModelObject( "content/models/sponza/sponza.mmdl" )
		{
			//rotation = new Vector3( 90, 0, 0 ),
			Scale = new Vector3( 0.025f )
		};
	}

	public void Update()
	{
		Entity.All.ForEach( entity => entity.Update() );
	}

	public void Render( CommandList commandList )
	{
		Entity.All.ForEach( entity => entity.Render( commandList ) );
	}
}
