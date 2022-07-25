using ImGuiNET;

namespace Mocha.Engine;

public class MaterialInspector : BaseInspector
{
	private Material material;

	public MaterialInspector( Material material )
	{
		this.material = material;
	}

	public override void Draw()
	{
		EditorHelpers.Title(
			$"{FontAwesome.FaceGrinStars} {Path.GetFileName( material.Path )}",
			"This is a material."
		);

		var items = new List<(string, string)>();
		foreach ( var property in material.GetType().GetProperties().Where( x => x.PropertyType == typeof( Texture ) ) )
		{
			var texture = property.GetValue( material ) as Texture;
			items.Add( (EditorHelpers.GetDisplayName( property.Name ), texture?.Path ?? "") );
		}

		EditorHelpers.TextBold( $"{FontAwesome.FaceGrinStars} Material" );

		DrawTable( items.ToArray() );

		EditorHelpers.Separator();

		DrawButtons( material.Path );
	}
}
