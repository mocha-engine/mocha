using static Mocha.UI.UIEntity;

namespace Mocha.UI;

public partial class UIModel : Model<UIVertex>
{
	public UIModel( UIVertex[] vertices, uint[] indices, Material material )
	{
		AddMesh( vertices, indices, material );
	}
}
