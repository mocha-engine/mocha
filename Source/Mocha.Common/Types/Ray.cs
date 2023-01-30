namespace Mocha.Common;
public struct Ray
{
	public Vector3 startPosition;
	public Vector3 direction;

	public Ray( Vector3 startPosition, Vector3 direction )
	{
		this.startPosition = startPosition;
		this.direction = direction;
	}
}
