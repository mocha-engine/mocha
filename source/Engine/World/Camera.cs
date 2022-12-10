namespace Mocha;

public static class Camera
{
	public static Vector3 Position
	{
		get => Glue.Entities.GetCameraPosition();
		set => Glue.Entities.SetCameraPosition( value );
	}

	public static Rotation Rotation
	{
		get => Glue.Entities.GetCameraRotation();
		set => Glue.Entities.SetCameraRotation( value );
	}

	public static float FieldOfView
	{
		get => Glue.Entities.GetCameraFieldOfView();
		set => Glue.Entities.SetCameraFieldOfView( value );
	}

	public static float ZNear
	{
		get => Glue.Entities.GetCameraZNear();
		set => Glue.Entities.SetCameraZNear( value );
	}

	public static float ZFar
	{
		get => Glue.Entities.GetCameraZFar();
		set => Glue.Entities.SetCameraZFar( value );
	}
}
