using static Mocha.Renderer.UI.PanelRenderer;

namespace Mocha.Renderer.UI;

public partial class UIModel : Model<UIVertex>
{
	public UIModel( UIVertex[] vertices, uint[] indices, Material material )
	{
		AddMesh( vertices, indices, material );
	}
}
