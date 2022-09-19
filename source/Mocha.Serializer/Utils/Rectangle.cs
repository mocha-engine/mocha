namespace Mocha.Common;

public struct Rectangle
{
	public float X { get; set; }
	public float Y { get; set; }

	public float Width { get; set; }
	public float Height { get; set; }

	public Vector2 Position => new Vector2( X, Y );
	public Vector2 Size => new Vector2( Width, Height );

	public Rectangle( float x, float y, float w, float h )
	{
		X = x;
		Y = y;

		Width = w;
		Height = h;
	}
	
	public static Rectangle operator +( Rectangle r, Vector2 p )
	{
		return new Rectangle( r.X + p.X, r.Y + p.Y, r.Width, r.Height );
	}
	
	public static Rectangle operator /( Rectangle r, Vector2 p )
	{
		return new Rectangle( r.X / p.X, r.Y / p.Y, r.Width / p.X, r.Height / p.Y );
	}

	public override string ToString()
	{
		return $"{X}, {Y} -> {Width}, {Height}";
	}
}
