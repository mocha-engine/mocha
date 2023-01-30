namespace Mocha.Common;

public interface IGame
{
	void Startup();
	void Shutdown();

	void FrameUpdate();
	void Update();
}
