namespace Mocha.Renderer.UI;

[Icon( FontAwesome.Square ), Title( "UI" )]
public partial class PanelRenderer
{
	public AtlasBuilder AtlasBuilder { get; set; }
	private Material Material { get; set; }
	private Model Model { get; set; }

	public PanelRenderer()
	{
		AtlasBuilder = new();

		Material = new( "core/shaders/ui/ui.mshdr", UIVertex.VertexAttributes );
	}

	public void NewFrame()
	{
		Vertices.Clear();
		RectCount = 0;
	}

	public void AddRectangle( Common.Rectangle rect, Common.Rectangle ndcTexRect, float screenPxRange, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD, GraphicsFlags flags )
	{
		if ( rect.X > Screen.Size.X || rect.Y > Screen.Size.Y )
			return;

		var ndcRect = rect / (Vector2)Screen.Size;
		var vertices = RectVertices.Select( ( x, i ) =>
		{
			var position = x.Position;
			position.X = (x.Position.X * ndcRect.Size.X) + ndcRect.Position.X;
			position.Y = (x.Position.Y * ndcRect.Size.Y) + ndcRect.Position.Y;

			var texCoords = x.TexCoords;
			texCoords.X = (x.TexCoords.X * ndcTexRect.Size.X) + ndcTexRect.Position.X;
			texCoords.Y = (x.TexCoords.Y * ndcTexRect.Size.Y) + ndcTexRect.Position.Y;

			var tx = x;
			position *= 2.0f;
			position.X -= 1.0f;
			position.Y = 1.0f - position.Y;

			tx.Position = position;
			tx.TexCoords = texCoords;
			tx.PanelPos *= rect.Size;
			tx.PanelSize = rect.Size;
			tx.Color = i switch
			{
				0 => colorA,
				1 => colorB,
				2 => colorC,
				3 => colorD,
				_ => Vector4.Zero,
			};
			tx.Flags = (short)flags;

			return tx;
		} ).ToArray();

		Vertices.AddRange( vertices );
		RectCount++;
	}
}
