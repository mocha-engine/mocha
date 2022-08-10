namespace Mocha;

[Category( "World" ), Icon( FontAwesome.Sun ), Title( "Sun" )]
public class Sun : BaseEntity
{
	[HideInInspector]
	public SceneLight SceneLight { get; set; }

	public float Intensity
	{
		get => SceneLight.Intensity;
		set => SceneLight.Intensity = value;
	}

	public Vector4 Color
	{
		get => SceneLight.Color;
		set => SceneLight.Color = value;
	}

	public Sun()
	{
		SceneLight = new( this );
	}
}
