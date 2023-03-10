namespace Mocha;

public static class Bobbing
{
	public static Vector3 CalculateOffset( float walkBob, float t, float factor = 1.0f )
	{
		Vector3 TargetOffset = new();

		TargetOffset += Vector3.Up * MathF.Sin( walkBob ) * t * -0.5f * factor;
		TargetOffset += Vector3.Left * MathF.Sin( walkBob * 0.5f ) * t * -0.5f * factor;

		return TargetOffset;
	}
}

public struct ViewModelOffset
{
	public Vector3 Position { get; set; }
	public Vector3 EulerRotation { get; set; }

	public Rotation Rotation => Rotation.From( EulerRotation );

	public ViewModelOffset( Vector3? position = null, Vector3? eulerRotation = null )
	{
		Position = position ?? default;
		EulerRotation = eulerRotation ?? default;
	}
}

public class ViewModel : ModelEntity
{
	private readonly float _walkBob;
	private readonly float _offsetLerpRate = 10f;
	private readonly float _fovLerpRate = 25f;

	private Vector3 _targetPosition;
	private Rotation _targetRotation;

	protected override void Spawn()
	{
		Model = new Model( "models/m4a1.mmdl" );
		Name = "ViewModel";
		Scale = new Vector3( 0.1f );
		IsViewModel = true;
	}

	public Dictionary<string, ViewModelOffset> Offsets = new()
	{
		{ "Base", new( new( 0.4f, 0.12f, -0.18f ) ) },
		{ "Sprint", new( new( 0.02f, 0.0f, 0.07f ), new( 30.89f, -11.25f, -5.6f ) ) },
		{ "Avoid", new( new( 0.02f, 0.0f, 0.07f ), new( 30.89f, -11.25f, -5.6f ) ) }
	};

	public override void Update()
	{
		_targetPosition = Vector3.Zero;
		_targetRotation = Rotation.Identity;

		BuildSprintEffects();
		BuildWalkEffects();
		BuildSwayEffects();
		BuildAvoidanceEffects();
		ApplyEffects();
	}

	private void BuildSwayEffects()
	{
		var delta = new Vector3( -Input.MouseDelta.Y, -Input.MouseDelta.X, 0 ) * 0.25f;
		_targetRotation *= Rotation.From( delta );
	}

	private void BuildAvoidanceEffects()
	{
		var player = Player.Local;
		var ray = Cast.Ray( player.EyeRay, 2.0f ).Ignore( player ).Run();

		_targetRotation = _targetRotation.LerpTo( Offsets["Avoid"].Rotation, 1.0f - ray.Fraction );
		_targetPosition = _targetPosition.LerpTo( Offsets["Avoid"].Position, 1.0f - ray.Fraction );
	}

	private void BuildWalkEffects()
	{
		var player = Player.Local;

		var speed = player.Velocity.WithZ( 0 ).Length;
		float t = speed.LerpInverse( 0, 4.0f );

		//if ( player.GroundEntity != null )
		//	WalkBob += Time.Delta * 20.0f * t;

		float factor = 0.025f;
		_targetPosition += Bobbing.CalculateOffset( _walkBob, t, factor ) * Camera.Rotation;
		_targetPosition += new Vector3( 0, 0, t ) * factor * 0.1f;
	}

	private bool BuildSprintEffects()
	{
		var player = Player.Local;

		//if ( player.WalkController.Sprinting )
		//{
		//	TargetRotation *= Offsets["Sprint"].Rotation;
		//	TargetPosition += Offsets["Sprint"].Position;
		//	return true;
		//}

		return false;
	}

	private Vector3 OffsetPosition;
	private Rotation OffsetRotation;

	private void ApplyEffects()
	{
		//
		// Interpolation
		//
		OffsetPosition = OffsetPosition.LerpTo( _targetPosition, 10f * Time.Delta );
		OffsetRotation = OffsetRotation.LerpTo( _targetRotation, 10f * Time.Delta );

		//
		// Set values
		//
		Rotation = Camera.Rotation;
		Rotation *= OffsetRotation;

		Position = Camera.Position;
		Position += Offsets["Base"].Position * Rotation;
		Position += OffsetPosition * Rotation;
	}
}
