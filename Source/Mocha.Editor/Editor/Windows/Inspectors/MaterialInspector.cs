namespace Mocha.Editor;

[Inspector<Material>]
public class MaterialInspector : BaseInspector
{
	private Material _material;

	public MaterialInspector( Material material )
	{
		this._material = material;
	}

	private void TextureSlot( string name, Texture texture )
	{
		var startPos = ImGui.GetCursorPos();

		ImGui.TableNextRow();
		ImGui.TableNextColumn();

		texture ??= Texture.MissingTexture;

		ImGuiX.Image( texture, new Vector2( 32f ) );
		ImGui.TableNextColumn();
		ImGui.Text( $"{name}" );
		ImGui.TableNextColumn();
		ImGui.Text( $"{texture.Path}\n{texture.Width}x{texture.Height}" );

		var rectSize = new System.Numerics.Vector2( 0, 32 );
		ImGui.SetCursorPos( startPos + new System.Numerics.Vector2( 0, 4 ) );

		if ( ImGui.Selectable( $"##select_{name}", false, ImGuiSelectableFlags.SpanAllColumns, rectSize ) )
			InspectorWindow.SetSelectedObject( texture );
	}

	public override void Draw()
	{
		var (windowWidth, windowHeight) = (ImGui.GetWindowWidth(), ImGui.GetWindowHeight());

		ImGuiX.InspectorTitle(
			$"{Path.GetFileName( _material.Path )}",
			"This is a material.",
			ResourceType.Material
		);

		DrawButtons( _material.Path );
		ImGuiX.Separator();

		if ( ImGui.BeginListBox( "##inspector_table", new( -1, 210 ) ) )
		{
			ImGuiX.TextBold( $"{FontAwesome.FaceGrinStars} Material" );

			if ( ImGui.BeginTable( $"##material_slots", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
			{
				ImGui.TableSetupColumn( "Preview", ImGuiTableColumnFlags.WidthFixed, 32f );
				ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 100f );
				ImGui.TableSetupColumn( "Value", ImGuiTableColumnFlags.WidthStretch, 1f );

				foreach ( var property in _material.GetType().GetProperties().Where( x => x.PropertyType == typeof( Texture ) ) )
				{
					var texture = property.GetValue( _material ) as Texture;

					TextureSlot( ImGuiX.GetDisplayName( property.Name ), texture );
				}

				ImGui.EndTable();
			}

			ImGui.EndListBox();
		}

		ImGui.SetCursorPosY( windowHeight - windowWidth - 10 );
		// ImGuiX.Image( _material.DiffuseTexture, new Vector2( windowWidth, windowWidth ) - new Vector2( 16, 0 ) );
	}
}
