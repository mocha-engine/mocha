namespace Mocha.Common;

public partial struct Rotation : IEquatable<Rotation>
{
	private System.Numerics.Quaternion internalQuaternion;

	public float X
	{
		readonly get => internalQuaternion.X;
		set => internalQuaternion.X = value;
	}

	public float Y
	{
		readonly get => internalQuaternion.Y;
		set => internalQuaternion.Y = value;
	}

	public float Z
	{
		readonly get => internalQuaternion.Z;
		set => internalQuaternion.Z = value;
	}

	public float W
	{
		readonly get => internalQuaternion.W;
		set => internalQuaternion.W = value;
	}

	public Rotation Normal => Normalize( this );
	public Vector3 Forward => this * Vector3.Forward;
	public Vector3 Backward => this * Vector3.Backward;
	public Vector3 Left => this * Vector3.Left;
	public Vector3 Right => this * Vector3.Right;
	public Vector3 Up => this * Vector3.Up;
	public Vector3 Down => this * Vector3.Down;

	public Rotation( float x, float y, float z, float w )
	{
		internalQuaternion = new( x, y, z, w );
	}

	public static Rotation Identity => new Rotation( 0F, 0F, 0F, 1F );

	public static Rotation From( float pitch, float yaw, float roll )
	{
		pitch = pitch.DegreesToRadians();
		yaw = yaw.DegreesToRadians();
		roll = roll.DegreesToRadians();

		float sp = MathF.Sin( pitch * 0.5f );
		float cp = MathF.Cos( pitch * 0.5f );
		float sy = MathF.Sin( yaw * 0.5f );
		float cy = MathF.Cos( yaw * 0.5f );
		float sr = MathF.Sin( roll * 0.5f );
		float cr = MathF.Cos( roll * 0.5f );

		float srcp = sr * cp;
		float crsp = cr * sp;
		float crcp = cr * cp;
		float srsp = sr * sp;

		var result = new Rotation(
			srcp * cy - crsp * sy,
			crsp * cy + srcp * sy,
			crcp * sy - srsp * cy,
			crcp * cy + srsp * sy
		);

		return result;
	}

	public Vector3 ToEulerAngles()
	{
		Vector3 angles = new();

		float x = 2f * W * W + 2f * X * X - 1f;
		float y = 2f * X * Y + 2f * W * Z;
		float num = 2f * X * Z - 2f * W * Y;
		float y2 = 2f * Y * Z + 2f * W * X;
		float x2 = 2f * W * W + 2f * Z * Z - 1f;

		angles.X = MathF.Asin( 0f - num ).RadiansToDegrees();
		angles.Y = MathF.Atan2( y, x ).RadiansToDegrees();
		angles.Z = MathF.Atan2( y2, x2 ).RadiansToDegrees();

		return angles;
	}

	public static Rotation operator *( Rotation a, Rotation b )
	{
		return new Rotation(
			a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
			a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
			a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
			a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
		);
	}

	public static Vector3 operator *( Rotation r, Vector3 v )
	{
		float x = r.X * 2F;
		float y = r.Y * 2F;
		float z = r.Z * 2F;

		float xx = r.X * x;
		float yy = r.Y * y;
		float zz = r.Z * z;

		float xy = r.X * y;
		float xz = r.X * z;
		float yz = r.Y * z;

		float wx = r.W * x;
		float wy = r.W * y;
		float wz = r.W * z;

		Vector3 result = new(
			(1F - (yy + zz)) * v.X + (xy - wz) * v.Y + (xz + wy) * v.Z,
			(xy + wz) * v.X + (1F - (xx + zz)) * v.Y + (yz - wx) * v.Z,
			(xz - wy) * v.X + (yz + wx) * v.Y + (1F - (xx + yy)) * v.Z
		);

		return result;
	}

	private static bool IsEqualUsingDot( float dot )
	{
		return dot > 1.0f - float.Epsilon;
	}

	public static bool operator ==( Rotation a, Rotation b )
	{
		return IsEqualUsingDot( Dot( a, b ) );
	}

	public static bool operator !=( Rotation a, Rotation b )
	{
		return !(a == b);
	}

	public static float Dot( Rotation a, Rotation b )
	{
		return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
	}

	public static Rotation LookAt( Vector3 forward, Vector3? _up = null )
	{
		var up = _up ?? Vector3.Up;

		// Normalize vectors
		forward = forward.Normal;
		up = up.Normal;

		float f = forward.Dot( up );
		up = (up - forward * f).Normal;

		var normal = forward.Cross( up ).Normal;
		var invNormal = -normal;

		float num = forward.X + invNormal.Y + up.Z;

		Rotation rot = new();
		if ( num >= 0f )
		{
			rot.X = invNormal.Z - up.Y;
			rot.Y = up.X - forward.Z;
			rot.Z = forward.Y - invNormal.X;
			rot.W = num + 1f;
		}
		else if ( forward.X > invNormal.Y && forward.X > up.Z )
		{
			rot.X = forward.X - invNormal.Y - up.Z + 1f;
			rot.Y = invNormal.X + forward.Y;
			rot.Z = up.X + forward.Z;
			rot.W = invNormal.Z - up.Y;
		}
		else if ( invNormal.Y > up.Z )
		{
			rot.X = forward.Y + invNormal.X;
			rot.Y = invNormal.Y - up.Z - forward.X + 1f;
			rot.Z = up.Y + invNormal.Z;
			rot.W = up.X - forward.Z;
		}
		else
		{
			rot.X = forward.Z + up.X;
			rot.Y = invNormal.Z + up.Y;
			rot.Z = up.Z - forward.X - invNormal.Y + 1f;
			rot.W = forward.Y - invNormal.X;
		}

		return rot;
	}

	public static float Angle( Rotation a, Rotation b )
	{
		float dot = MathF.Min( MathF.Abs( Dot( a, b ) ), 1.0F );
		return IsEqualUsingDot( dot ) ? 0.0f : (MathF.Acos( dot ) * 2.0F).RadiansToDegrees();
	}

	public static Rotation Normalize( Rotation r )
	{
		float mag = MathF.Sqrt( Dot( r, r ) );

		if ( mag < float.Epsilon )
			return Identity;

		return new Rotation( r.X / mag, r.Y / mag, r.Z / mag, r.W / mag );
	}

	public override bool Equals( object obj )
	{
		if ( obj is not Rotation )
			return false;

		return Equals( (Rotation)obj );
	}

	public bool Equals( Rotation rot )
	{
		return X.Equals( rot.X )
			&& Y.Equals( rot.Y )
			&& Z.Equals( rot.Z )
			&& W.Equals( rot.W );
	}

	public override string ToString() => internalQuaternion.ToString();
	public System.Numerics.Quaternion GetSystemQuaternion() => internalQuaternion;
}
