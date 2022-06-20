namespace Mocha.Common;

public interface IEntity
{
	public int Id { get; set; }
	public Transform Transform { get; set; }

	void Delete();
}
