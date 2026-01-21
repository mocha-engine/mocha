namespace Mocha.Common;

/// <summary>
/// Represents a client connected to a server.
/// </summary>
public interface IClient
{
	public abstract string Name { get; set; }
	public abstract int Ping { get; set; }
	public abstract IEntity Pawn { get; set; }
}
