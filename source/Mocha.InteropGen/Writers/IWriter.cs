namespace Mocha.InteropGen;

public interface IWriter : IDisposable
{
	void Write( string str );
	void WriteLine( string str );
	void WriteLine();
}
