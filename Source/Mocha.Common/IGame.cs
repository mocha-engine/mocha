namespace Mocha.Common;

public interface IGame
{
	void Startup();
	void Shutdown();

	/// <summary>
	/// Called every frame on the client.
	/// Note that there is nothing for this on the server, because servers don't render anything.
	/// </summary>
	void FrameUpdate();

	/// <summary>
	/// Called every tick on the client and the server.
	/// </summary>
	void Update();
}
