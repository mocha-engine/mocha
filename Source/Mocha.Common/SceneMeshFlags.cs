namespace Mocha;

public enum SceneMeshFlags
{
	WorldLayer = 1 << 1,
	UILayer = 1 << 2,

	Default = WorldLayer,
	PostProcess = UILayer,
};
