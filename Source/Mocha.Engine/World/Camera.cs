namespace Mocha;

public static class Camera
{
	public static Vector3 Position
	{
		get => NativeEngine.GetCameraPosition();
		set => NativeEngine.SetCameraPosition( value );
	}

	public static Rotation Rotation
	{
		get => NativeEngine.GetCameraRotation();
		set => NativeEngine.SetCameraRotation( value );
	}

	public static float FieldOfView
	{
		get => NativeEngine.GetCameraFieldOfView();
		set => NativeEngine.SetCameraFieldOfView( value );
	}

	public static float ZNear
	{
		get => NativeEngine.GetCameraZNear();
		set => NativeEngine.SetCameraZNear( value );
	}

	public static float ZFar
	{
		get => NativeEngine.GetCameraZFar();
		set => NativeEngine.SetCameraZFar( value );
	}
}
