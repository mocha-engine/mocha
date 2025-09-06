namespace Mocha.Common;

public interface IActor
{
	string Name { get; set; }
	Transform Transform { get; set; }

	void Delete();
	void Update();
}

public static class IActorExtensions
{
	public static bool IsValid( this IActor actor )
	{
		return actor is not null;
	}
}
