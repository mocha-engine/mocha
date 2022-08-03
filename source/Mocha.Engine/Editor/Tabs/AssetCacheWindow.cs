using ImGuiNET;
using System.ComponentModel;

namespace Mocha.Engine;

[Icon( FontAwesome.Database ), Title( "Asset Cache" ), Category( "Engine" )]
internal class AssetCacheWindow : BaseEditorWindow
{
	public AssetCacheWindow()
	{

	}

	public override void Draw()
	{
		if ( ImGui.Begin( "Asset Cache", ref isVisible ) )
		{
			EditorHelpers.Title(
				$"{FontAwesome.Database} Asset Cache",
				"Here's where you can see all the currently cached assets."
			);

			ImGui.BeginListBox( "##textures", new System.Numerics.Vector2( -1, -48 ) );
			var assetList = Asset.All.ToList();

			for ( int i = 0; i < assetList.Count; i++ )
			{
				var asset = assetList[i];
				var displayInfo = DisplayInfo.For( asset );
				var path = asset?.Path;

				if ( ImGui.Selectable( $"{displayInfo.CombinedTitle.Pad()} {path}" ) )
				{
					var selectedAsset = assetList[i];
					InspectorWindow.SetSelectedObject( selectedAsset );
				}
			}

			ImGui.EndListBox();

			EditorHelpers.Separator();

			ImGui.Text( $"Cached assets: {Asset.All.Count()}" );
			ImGui.End();
		}
	}
}
