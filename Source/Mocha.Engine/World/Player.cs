namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : Actor
{
	[HideInInspector]
	public static Player? Local => Actor.All.OfType<Player>().FirstOrDefault();

	[HideInInspector]
	public Ray EyeRay => new Ray( EyePosition, EyeRotation.Forward );

	[Category( "Player" )]
	public Vector3 PlayerBounds { get; set; }

	[Category( "Player" )]
	public Vector3 EyePosition { get; set; }

	[Category( "Player" )]
	public Rotation EyeRotation { get; set; }

	[Category( "Player" )]
	public Vector3 LocalEyePosition { get; set; }

	[Category( "Player" )]
	public Rotation LocalEyeRotation { get; set; }

	protected override void Spawn()
	{
		base.Spawn();

		/*Restitution = 0.0f;
		Friction = 1.0f;
		Mass = 100f;
		IgnoreRigidbodyRotation = true;*/

		// ViewModel = new();
		Respawn();
	}

	public virtual void Respawn()
	{
	}
}
