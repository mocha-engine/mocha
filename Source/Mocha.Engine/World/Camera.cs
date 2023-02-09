namespace Mocha;

public static class Camera
{
	public static Vector3 Position
	{
		get => Engine.GetCameraPosition();
		set => Engine.SetCameraPosition( value );
	}

	public static Rotation Rotation
	{
		get => Engine.GetCameraRotation();
		set => Engine.SetCameraRotation( value );
	}

	public static float FieldOfView
	{
		get => Engine.GetCameraFieldOfView();
		set => Engine.SetCameraFieldOfView( value );
	}

	public static float ZNear
	{
		get => Engine.GetCameraZNear();
		set => Engine.SetCameraZNear( value );
	}

	public static float ZFar
	{
		get => Engine.GetCameraZFar();
		set => Engine.SetCameraZFar( value );
	}
}
