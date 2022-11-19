namespace Mocha.Common;

public class Logger
{
	public Glue.Logger? NativeLogger { get; set; }

	public void Trace( object obj ) => NativeLogger?.Trace( obj?.ToString() );
	public void Info( object obj ) => NativeLogger?.Info( obj?.ToString() );
	public void Warning( object obj ) => NativeLogger?.Warning( obj?.ToString() );
	public void Error( object obj ) => NativeLogger?.Error( obj?.ToString() );
}
