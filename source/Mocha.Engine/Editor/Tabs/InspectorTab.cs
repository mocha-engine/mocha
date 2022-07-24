using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.Globe} World/Inspector" )]
internal class InspectorTab : BaseTab
{
	private static InspectorTab Instance { get; set; }
	private BaseInspector Inspector { get; set; }

	public static void SetSelectedObject( object obj )
	{
		// TODO: Use reflection for this

		if ( obj is Entity entity )
			Instance.Inspector = new EntityInspector( entity );
		else if ( obj is Texture texture )
			Instance.Inspector = new AssetInspector( texture );
		else if ( obj is Shader shader )
			Instance.Inspector = new ShaderInspector( shader );
		else if ( obj is List<Model> model )
			Instance.Inspector = new ModelInspector( model.First() );
	}

	public InspectorTab()
	{
		Instance = this;
		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( $"Inspector" );

		Inspector?.Draw();

		ImGui.End();
	}
}
