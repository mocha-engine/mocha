using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Image, $"{FontAwesome.Gears} Engine/Textures" )]
internal class TexturesTab : BaseTab
{
	public TexturesTab()
	{

	}

	private int selectedTexture;

	public override void Draw()
	{
		if ( ImGui.Begin( "Texture Debug", ref isVisible ) )
		{
			EditorHelpers.Title(
				$"{FontAwesome.Image} Textures",
				"Here's where you can see all the currently loaded textures."
			);

			EditorHelpers.TextLight( $"Loaded textures: {Asset.All.OfType<Texture>().Count()}" );

			ImGui.Dummy( new System.Numerics.Vector2( 0, 4 ) );

			ImGui.BeginListBox( "##textures", new System.Numerics.Vector2( 250, -1 ) );
			var textureList = Asset.All.OfType<Texture>().ToList();

			for ( int i = 0; i < textureList.Count; i++ )
			{
				Texture? tex = textureList[i];
				EditorHelpers.Image( tex, new Vector2( 32, 32 ) );
				ImGui.SameLine();
				if ( ImGui.Selectable( tex.Path + "\n" + tex.Width + ", " + tex.Height + " | " + tex.Type ) )
				{
					selectedTexture = i;
				}
			}

			ImGui.EndListBox();

			if ( selectedTexture > textureList.Count - 1 )
				selectedTexture = textureList.Count - 1;

			if ( selectedTexture < 0 )
				selectedTexture = 0;

			var texture = textureList[selectedTexture];

			ImGui.SameLine();
			ImGui.BeginChild( "##texture_preview" );

			var childWidth = ImGui.GetItemRectSize().X;
			float ratio = texture.Height / (float)texture.Width;

			if ( ratio is float.NaN )
				ratio = 1f;

			EditorHelpers.Image( texture, new Vector2( childWidth, childWidth * ratio ) );

			ImGui.EndChild();
			ImGui.End();
		}
	}
}
