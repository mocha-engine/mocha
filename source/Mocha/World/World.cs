using Mocha.UI;
using Veldrid;

namespace Mocha;

public class World
{
	public static World Current { get; set; }

	public RootPanel Hud { get; set; }
	public Camera Camera { get; set; }

	public Sun Sun { get; set; }

	public World()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );

		SetupEntities();
		SetupHud();
	}

	private void SetupEntities()
	{
		Camera = new Camera();
		Sun = new Sun() { position = new( 0, 10, 10 ) };
		_ = new GenericModelObject( "content/models/sponza/sponza.obj" );
	}

	private void SetupHud()
	{
		Hud = new();
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
