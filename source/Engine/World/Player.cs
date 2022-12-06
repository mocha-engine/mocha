namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	private float velocity;

	public Player()
	{
		Scale = new Vector3( 0.025f );
		Rotation = Rotation.Identity;
		Position = new( 32, 0, 0 );
	}

	public override void Update()
	{
		base.Update();

		if ( Input.Pressed( InputButton.Jump ) )
		{
			velocity = 15f;
			Rotation = Rotation.From( 0, 0, Rotation.ToEulerAngles().Z - 45f );
		}

		velocity = velocity.LerpTo( -15f, Time.Delta );
		Position += Vector3.Up * velocity * Time.Delta;

		if ( Position.Z < -16 )
			Position = new( 32, 0, 0 );

		if ( Position.Z > 16 )
			Position = new( 32, 0, 0 );

		float roll = Time.Delta * 90f + Rotation.ToEulerAngles().Z;
		roll = roll.LerpTo( 90.0f, Time.Delta * 10f );

		Rotation = Rotation.From( 0, 0, roll );
	}
}
