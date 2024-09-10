namespace Mocha;

public struct PhysicsBody
{
	public Vector3 Velocity;
	public Vector3 AngularVelocity;

	public float Friction;
	public float Restitution;
	public float Mass;

	public static PhysicsBody Cube( Vector3 bounds )
	{
		return new();
	}

	public static PhysicsBody Sphere( Vector3 bounds )
	{
		return new();
	}

	public static PhysicsBody Mesh( Vector3 bounds )
	{
		return new();
	}
}
