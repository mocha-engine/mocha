using System.ComponentModel;

namespace Mocha.Engine;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	public Player() : base( "game/models/subaru/subaru.mmdl" )
	{
		Scale = new Vector3( 0.025f );
		Rotation = Rotation.Identity;
		Position = new( 32, 0, 0 );
	}

	public override void Update()
	{
		base.Update();

		Position = Vector3.Zero;
		Rotation = Rotation.From( 0, 90, 90 ) 
			* Rotation.From( ( 180 * Time.Now ) % 360f, 0, 0 );
	}
}
