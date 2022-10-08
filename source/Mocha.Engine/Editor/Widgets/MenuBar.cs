using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class MenuBar : Widget
{
	public Vector2 TextAnchor = new Vector2( 0.5f, 0.5f );
	private Vector2 Padding => new Vector2( 0, 20 );

	public MenuBar() : base()
	{
	}

	internal override void Render()
	{
		Vector4 colorA = ITheme.Current.ButtonBgA;
		Vector4 colorB = ITheme.Current.ButtonBgB;
		Vector4 border = ITheme.Current.Border;

		Graphics.DrawShadow( Bounds, 8f, ITheme.Current.ShadowOpacity );
		Graphics.DrawRect( Bounds, border );
		Graphics.DrawRect( Bounds + new Vector2( 0, -1 ),
			colorA,
			colorB,
			colorA,
			colorB
		);
	}

	internal override Vector2 GetDesiredSize()
	{
		var size = new Vector2( 128, Padding.Y * 2 );
		return size;
	}
}
