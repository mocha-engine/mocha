﻿using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( "Debug/Textures" )]
internal class TexturesTab : BaseTab
{
	public TexturesTab()
	{

	}

	private int selectedTexture;

	public override void Draw()
	{
		ImGui.Begin( "Textures", ref isVisible );

		var textureList = Asset.All.OfType<Texture>().ToList();

		var texturePaths = textureList.Select( texture => $"{texture.Path}" ).ToList();

		ImGui.Combo( "Texture", ref selectedTexture, texturePaths.ToArray(), texturePaths.Count );

		if ( selectedTexture > textureList.Count - 1 )
			selectedTexture = textureList.Count - 1;

		if ( selectedTexture < 0 )
			selectedTexture = 0;

		var texture = textureList[selectedTexture];

		var windowWidth = ImGui.GetWindowSize().X;
		float ratio = texture.Height / (float)texture.Width;

		if ( ratio is float.NaN )
			ratio = 1f;

		EditorHelpers.Image( texture, new Vector2( windowWidth, windowWidth * ratio ) );

		ImGui.End();
	}
}
