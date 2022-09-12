namespace Mocha.Renderer;

public class SceneCamera : SceneObject
{
	public Framebuffer Framebuffer { get; set; }

	public Texture ColorTexture { get; private set; }
	public Texture DepthTexture { get; private set; }

	private Point2 CurrentSize { get; set; }

	public SceneCamera()
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
			.WithName( $"SceneCamera depth" )
			.Build();

		ColorTexture = Texture.Builder
			.FromEmpty( (uint)newSize.X, (uint)newSize.Y )
			.AsColorAttachment()
			.IgnoreCache()
			.WithName( $"SceneCamera color" )
			.Build();

		var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture, ColorTexture.VeldridTexture );
		Framebuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );

		Log.Trace( $"Resized to {newSize}" );

		CurrentSize = newSize;
	}
	
	public void BuildCamera( ref CameraSetup cameraSetup )
	{
		cameraSetup.AspectRatio = (float)CurrentSize.X / (float)CurrentSize.Y;
	}
}
