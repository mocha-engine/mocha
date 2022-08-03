using ImGuiNET;

namespace Mocha.Engine;

public class MaterialInspector : BaseInspector
{
	private Material material;

	public MaterialInspector( Material material )
	{
		this.material = material;
	}

	private void TextureSlot( string name, Texture texture )
	{
		var startPos = ImGui.GetCursorPos();

		ImGui.TableNextRow();
		ImGui.TableNextColumn();

		texture ??= TextureBuilder.MissingTexture;

		ImGuiX.Image( texture, new Vector2( 32f ) );
		ImGui.TableNextColumn();
		ImGui.Text( $"{name}\n{texture.Type}" );
		ImGui.TableNextColumn();
		ImGui.Text( $"{texture.Path}\n{texture.Width}x{texture.Height}" );

		var rectSize = new System.Numerics.Vector2( 0, 32 );
		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( 0, 4 ) );

		if ( ImGui.Selectable( $"##select_{name}", false, ImGuiSelectableFlags.SpanAllColumns, rectSize ) )
			InspectorWindow.SetSelectedObject( texture );
	}

	public override void Draw()
	{
		var windowWidth = ImGui.GetWindowWidth();

		ImGuiX.Title(
			$"{FontAwesome.FaceGrinStars} {Path.GetFileName( material.Path )}",
			"This is a material."
		);

		ImGuiX.Image( material.DiffuseTexture ?? TextureBuilder.MissingTexture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
		ImGuiX.Separator();

		ImGui.BeginListBox( "##inspector_table", new( -1, 210 ) );

		ImGuiX.TextBold( $"{FontAwesome.FaceGrinStars} Material" );

		if ( ImGui.BeginTable( $"##material_slots", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
		{
			ImGui.TableSetupColumn( "Preview", ImGuiTableColumnFlags.WidthFixed, 32f );
			ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 100f );
			ImGui.TableSetupColumn( "Value", ImGuiTableColumnFlags.WidthStretch, 1f );

			foreach ( var property in material.GetType().GetProperties().Where( x => x.PropertyType == typeof( Texture ) ) )
			{
				var texture = property.GetValue( material ) as Texture;

				TextureSlot( ImGuiX.GetDisplayName( property.Name ), texture );
			}

			ImGui.EndTable();
		}

		ImGui.EndListBox();

		ImGuiX.Separator();

		DrawButtons( material.Path );
	}
}
