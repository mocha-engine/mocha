using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.List, $"{FontAwesome.Gamepad} Game/Inspector" )]
internal class InspectorWindow : BaseEditorWindow
{
	private static InspectorWindow Instance { get; set; }
	private BaseInspector Inspector { get; set; }

	public static void SetSelectedObject( object obj )
	{
		// TODO: Use reflection for this

		if ( obj is Entity entity )
			Instance.Inspector = new EntityInspector( entity );
		else if ( obj is Texture texture )
			Instance.Inspector = new TextureInspector( texture );
		else if ( obj is Shader shader )
			Instance.Inspector = new ShaderInspector( shader );
		else if ( obj is Model model )
			Instance.Inspector = new ModelInspector( model );
		else if ( obj is List<Model> modelList )
			Instance.Inspector = new ModelInspector( modelList.First() );
		else if ( obj is Material material )
			Instance.Inspector = new MaterialInspector( material );
	}

	public InspectorWindow()
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
