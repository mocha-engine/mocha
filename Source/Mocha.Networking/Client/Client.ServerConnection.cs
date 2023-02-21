namespace Mocha.Networking;

public partial class Client
{
	public readonly struct ServerConnection : IConnection
	{
		public void Disconnect( string reason )
		{
			throw new NotImplementedException();
		}
	}
}
