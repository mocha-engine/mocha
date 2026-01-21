using Mocha.Common;

namespace Mocha.Networking;

internal class ConnectedClient : IClient
{
	public string Name { get; set; }
	public int Ping { get; set; }
	public IEntity Pawn { get; set; }
}
