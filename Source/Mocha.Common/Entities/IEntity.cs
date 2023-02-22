namespace Mocha.Common;

public interface IEntity
{
	string Name { get; set; }
	uint NativeHandle { get; }
	NetworkId NetworkId { get; }

	Vector3 Position { get; set; }
	Rotation Rotation { get; set; }
	Vector3 Scale { get; set; }

	void Delete();
	void Delete( bool immediate );
	bool IsValid();
	void Update();
}
