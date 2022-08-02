using Mocha.Common.World;

namespace Mocha.Renderer;

public class SceneCamera : SceneObject
{
	public Matrix4x4 ViewMatrix { get; set; }
	public Matrix4x4 ProjMatrix { get; set; }
	public Framebuffer Framebuffer { get; set; }

	public Texture ColorTexture { get; set; }
	public Texture DepthTexture { get; set; }

	public float FieldOfView { get; set; } = 60;

	private Point2 CurrentSize { get; set; }

	public SceneCamera( IEntity entity ) : base( entity )
	{
		UpdateSize( Window.Current.Size );
	}

	public void UpdateAspect( Point2 newSize )
	{
		if ( newSize.X == CurrentSize.X && newSize.Y == CurrentSize.Y )
			return;

		CurrentSize = newSize;
	}

	// TODO: Make it so that we can call this in ViewportTab etc. without running OOM
	private void UpdateSize( Point2 newSize )
	{
		if ( newSize.X == CurrentSize.X && newSize.Y == CurrentSize.Y )
			return;

		Framebuffer?.Dispose();

		DepthTexture = Texture.Builder
			.FromEmpty( (uint)newSize.X, (uint)newSize.Y )
			.AsDepthAttachment()
			.IgnoreCache()
			.WithName( $"SceneCamera {Entity.Id} depth" )
			.Build();

		ColorTexture = Texture.Builder
			.FromEmpty( (uint)newSize.X, (uint)newSize.Y )
			.AsColorAttachment()
			.IgnoreCache()
			.WithName( $"SceneCamera {Entity.Id} color" )
			.Build();

		var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture, ColorTexture.VeldridTexture );
		Framebuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );

		Log.Trace( $"Resized to {newSize}" );

		CurrentSize = newSize;
	}

	public void CalcViewProjMatrix()
	{
		FieldOfView = FieldOfView.Clamp( 1, 179 );

		var cameraPos = Transform.Position;
		var cameraFront = Transform.Rotation.Forward;
		var cameraUp = new Vector3( 0, 0, 1 );

		ViewMatrix = Matrix4x4.CreateLookAt( cameraPos, cameraPos + cameraFront, cameraUp );
		ProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
			FieldOfView.DegreesToRadians(),
			(float)CurrentSize.X / (float)CurrentSize.Y,
			0.1f,
			1000.0f
		);
	}
}
