using Mocha.UI;

namespace Mocha;

public class BaseGame : IGame
{
	public static BaseGame Current { get; set; }

	public BaseGame()
	{
		Current = this;
		Event.Register( this );
		Event.RegisterStatics();
		Event.Run( Event.Game.LoadAttribute.Name );
	}

	public virtual void FrameUpdate()
	{
		UIManager.Instance.Render();
		BaseEntity.All.ToList().ForEach( entity => entity.FrameUpdate() );
	}

	public virtual void Update()
	{
		BaseEntity.All.ToList().ForEach( entity => entity.Update() );
	}

	public virtual void Shutdown()
	{
	}

	public virtual void Startup()
	{
	}
}
