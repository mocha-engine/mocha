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

	public Matrix4x4 BuildMatrix()
	{
		var matrix = Matrix4x4.CreateFromQuaternion( Rotation.GetSystemQuaternion() );
		matrix *= Matrix4x4.CreateScale( Scale );
		matrix *= Matrix4x4.CreateTranslation( Position );

		return matrix;
	}

	public Transform WithPosition( Vector3 v )
	{
		var tx = new Transform( this );
		tx.Position = v;
		return tx;
	}
}
