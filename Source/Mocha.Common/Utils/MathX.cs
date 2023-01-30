using System.Drawing;
using System.Numerics;

namespace Mocha.Common;

public static class MathX
{
	public static int CeilToInt( this float x ) => (int)Math.Ceiling( x );
	public static int FloorToInt( this float x ) => (int)Math.Floor( x );
	public static int RoundToInt( this float x ) => (int)Math.Round( x );

	public static int NearestPowerOf2( this int x ) => NearestPowerOf2( (uint)x );
	public static int NearestPowerOf2( this uint x ) => 1 << (sizeof( uint ) * 8 - BitOperations.LeadingZeroCount( x - 1 ));

	public static float DegreesToRadians( this float degrees ) => degrees * 0.0174533f;
	public static float RadiansToDegrees( this float radians ) => radians * 57.2958f;

	public static float Clamp( this float v, float min, float max )
	{
		if ( min > max )
			return max;
		if ( v < min )
			return min;
		return v > max ? max : v;
	}

	public static float LerpTo( this float a, float b, float t )
	{
		return a * (1 - t) + b * t.Clamp( 0, 1 );
	}

	public static float LerpInverse( this float t, float a, float b )
	{
		return ((t - a) / (b - a)).Clamp( 0, 1 );
	}

	public static Vector3 Normalize( this Vector3 vector ) => vector / vector.Length;

	public static Vector3 RandomVector3( float min = 0.0f, float max = 1.0f )
	{
		float x = Random.Shared.NextSingle() * (max - min) + min;
		float y = Random.Shared.NextSingle() * (max - min) + min;
		float z = Random.Shared.NextSingle() * (max - min) + min;

		return new Vector3( x, y, z );
	}

	public static System.Numerics.Vector4 GetColor( string hex )
	{
		var color = ColorTranslator.FromHtml( hex );

		return new System.Numerics.Vector4(
				(color.R / 255f),
				(color.G / 255f),
				(color.B / 255f),
				(color.A / 255f)
		);
	}

	public static Vector4 Column1( this Matrix4x4 matrix ) => new Vector4( matrix.M11, matrix.M21, matrix.M31, matrix.M41 );
	public static Vector4 Column2( this Matrix4x4 matrix ) => new Vector4( matrix.M12, matrix.M22, matrix.M32, matrix.M42 );
	public static Vector4 Column3( this Matrix4x4 matrix ) => new Vector4( matrix.M13, matrix.M23, matrix.M33, matrix.M43 );
	public static Vector4 Column4( this Matrix4x4 matrix ) => new Vector4( matrix.M14, matrix.M24, matrix.M34, matrix.M44 );

	public static Vector3 Right( this Matrix4x4 matrix ) => new Vector3( matrix.M11, matrix.M21, matrix.M31 );
	public static Vector3 Up( this Matrix4x4 matrix ) => new Vector3( matrix.M12, matrix.M22, matrix.M32 );
	public static Vector3 Forward( this Matrix4x4 matrix ) => -new Vector3( matrix.M13, matrix.M23, matrix.M33 );

	// https://stackoverflow.com/a/22733709/8176082
	public enum SizeUnits
	{
		Byte, KB, MB, GB, TB, PB, EB, ZB, YB
	}

	public static string ToSize( this long value, SizeUnits unit ) => (value / (double)Math.Pow( 1024, (long)unit )).ToString( "0.00" ) + unit.ToString();

	public static float NormalizeDegrees( this float d )
	{
		d %= 360f;
		if ( d < 0f )
			d += 360f;
		return d;
	}

	public static int CalcMipSize( int baseWidth, int mipNumber ) => (int)MathF.Max( 1, MathF.Floor( baseWidth / MathF.Pow( 2, mipNumber ) ) );

	public static float Sin01( float t ) => (float)Math.Sin( t * Math.PI * 2 ) * 0.5f + 0.5f;
}
