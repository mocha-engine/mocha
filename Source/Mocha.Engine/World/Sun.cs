namespace Mocha;

[Category( "World" ), Icon( FontAwesome.Sun ), Title( "Sun" )]
public class Sun : BaseEntity
{
	public float Intensity
	{
		get;
		set;
	}

	public Vector4 Color
	{
		get;
		set;
	}

	public Sun()
	{
	}
}
