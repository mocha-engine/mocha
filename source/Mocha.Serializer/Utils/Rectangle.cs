namespace Mocha.Common;

public struct Rectangle
{
	public float X { get; set; }
	public float Y { get; set; }

	public float Width { get; set; }
	public float Height { get; set; }

	public Vector2 Position
	{
		get => new Vector2( X, Y );
		set => (X, Y) = value;
	}

	public Vector2 Size
	{
		get => new Vector2( Width, Height );
		set => (Width, Height) = value;
	}

	public Rectangle( Vector2 position, Vector2 size )
	{
		X = position.X;
		Y = position.Y;

		Width = size.X;
		Height = size.Y;
	}

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

	public bool Contains( Vector2 v )
	{
		if ( v.X >= this.X && v.Y >= this.Y
			&& v.X < this.X + this.Width
			 && v.Y < this.Y + this.Height )
			return true;

		return false;
	}

	public override string ToString()
	{
		return $"Position: {X}, {Y} | Size: {Width}, {Height}";
	}

	public Rectangle Expand( Vector2 v )
	{
		return new Rectangle( this.X - v.X, this.Y - v.Y, this.Width + v.X*2, this.Height + v.Y*2 );
	}

	public Rectangle Shrink( Vector2 v )
	{
		return new Rectangle( this.X + v.X, this.Y + v.Y, this.Width - v.X*2, this.Height - v.Y*2 );
	}
}
