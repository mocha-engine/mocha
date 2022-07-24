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

    public SceneCamera( IEntity entity ) : base( entity )
    {
        uint resX = (uint)Window.Current.Size.X;
        uint resY = (uint)Window.Current.Size.Y;

        DepthTexture = Texture.Builder
            .FromEmpty( resX, resY )
            .AsDepthAttachment()
            .WithName( $"Camera {GetHashCode()} depth" )
            .Build();

        ColorTexture = Texture.Builder
            .FromEmpty( resX, resY )
            .AsColorAttachment()
            .WithName( $"Camera {GetHashCode()} color" )
            .Build();

        var framebufferDescription = new FramebufferDescription( DepthTexture.VeldridTexture, ColorTexture.VeldridTexture );

        Framebuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
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
            Screen.Aspect,
            0.1f,
            1000.0f
        );
    }
}
