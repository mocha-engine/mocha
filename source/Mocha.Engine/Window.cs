namespace Mocha.Engine;

public class Window
{
	private Glue.CNativeWindow NativeWindow { get; }
	public static Window Current { get; set; }
	public Point2 Size => new Point2( 1280, 720 );

	public Window()
	{
		Current ??= this;

		NativeWindow = new( "Mocha", 1280, 720 );
		Screen.UpdateFrom( Size );
	}

	public void Run()
	{
		NativeWindow.Run();
	}
}
