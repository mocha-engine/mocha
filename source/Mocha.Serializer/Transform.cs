namespace Mocha.Common;

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
}
