namespace Mocha;

[Category( "Player" ), Icon( FontAwesome.User )]
public class Player : ModelEntity
{
	[HideInInspector]
	public static Player? Local => BaseEntity.All.OfType<Player>().FirstOrDefault();

	[HideInInspector]
	public Ray EyeRay => new Ray( EyePosition, EyeRotation.Forward );

	[Category( "Player" )]
	public Vector3 PlayerHalfExtents { get; set; }

	[Category( "Player" )]
	public Vector3 EyePosition { get; set; }

	[Category( "Player" )]
	public Rotation EyeRotation { get; set; }

	[Category( "Player" )]
	public Vector3 LocalEyePosition { get; set; }

	[Category( "Player" )]
	public Rotation LocalEyeRotation { get; set; }

	[Category( "Player" )]
	public ViewModel ViewModel { get; set; }

	protected override void Spawn()
	{
		base.Spawn();

		Restitution = 0.0f;
		Friction = 1.0f;
		Mass = 100f;
		IgnoreRigidbodyRotation = true;

		// ViewModel = new();
		Respawn();
	}

	public virtual void Respawn()
	{
	}
}
