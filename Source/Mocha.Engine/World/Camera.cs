namespace Mocha;

public static class Camera
{
	public static Vector3 Position
	{
		get => Glue.Engine.GetCameraPosition();
		set => Glue.Engine.SetCameraPosition( value );
	}

	public static Rotation Rotation
	{
		get => Glue.Engine.GetCameraRotation();
		set => Glue.Engine.SetCameraRotation( value );
	}

	public static float FieldOfView
	{
		get => Glue.Engine.GetCameraFieldOfView();
		set => Glue.Engine.SetCameraFieldOfView( value );
	}

	public static float ZNear
	{
		get => Glue.Engine.GetCameraZNear();
		set => Glue.Engine.SetCameraZNear( value );
	}

	public static float ZFar
	{
		get => Glue.Engine.GetCameraZFar();
		set => Glue.Engine.SetCameraZFar( value );
	}
}
