namespace Mocha.Common;

public interface IGame
{
	internal void Startup();
	internal void Shutdown();

	/// <summary>
	/// Called every frame on the client.
	/// Note that there is nothing for this on the server, because servers don't render anything.
	/// </summary>
	internal void FrameUpdate();

	/// <summary>
	/// Called every tick on the client and the server.
	/// </summary>
	internal void Update();
}
