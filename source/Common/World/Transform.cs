namespace Mocha.Common.World;

public struct Transform
{
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public Vector3 Scale { get; set; }

	public Transform()
	{
		Position = Vector3.Zero;
		Rotation = Rotation.Identity;
		Scale = Vector3.One;
	}

	public Transform( Transform other )
	{
		Position = other.Position;
		Rotation = other.Rotation;
		Scale = other.Scale;
	}
}
