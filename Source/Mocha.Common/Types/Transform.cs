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

	public Transform( Transform other )
	{
		Position = other.Position;
		Rotation = other.Rotation;
		Scale = other.Scale;
	}

	public Transform WithPosition( Vector3 position )
	{
		var tx = new Transform
		{
			Position = position,
			Rotation = Rotation,
			Scale = Scale
		};

		return tx;
	}

	public Transform WithRotation( Rotation rotation )
	{
		var tx = new Transform
		{
			Position = Position,
			Rotation = rotation,
			Scale = Scale
		};

		return tx;
	}

	public Transform WithScale( Vector3 scale )
	{
		var tx = new Transform
		{
			Position = Position,
			Rotation = Rotation,
			Scale = scale
		};

		return tx;
	}

	public static Transform Default => new Transform()
	{
		Position = Vector3.Zero,
		Rotation = Rotation.Identity,
		Scale = Vector3.One
	};
}
