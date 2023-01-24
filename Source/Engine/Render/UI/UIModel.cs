using static Mocha.UIEntity;

namespace Mocha;

public partial class UIModel : Model<UIVertex>
{
	public UIModel( UIVertex[] vertices, uint[] indices, Material material )
	{
		AddMesh( vertices, indices, material );
	}
}
