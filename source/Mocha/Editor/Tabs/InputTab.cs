using ImGuiNET;

namespace Mocha;

[EditorMenu( "Debug/Input" )]
internal class InputTab : BaseTab
{
	public override void Draw()
	{
		ImGui.Begin( "Input", ref visible );

		ImGui.Text( $"Time: {Time.Now}" );

		ImGui.Text( $"Last keys down:" );
		Input.Snapshot.LastKeysDown.ForEach( key => ImGui.Text( $"\t{key}" ) );

		ImGui.Text( $"\n\nKeys down:" );
		Input.Snapshot.KeysDown.ForEach( key => ImGui.Text( $"\t{key}" ) );

		foreach ( var prop in typeof( Input ).GetProperties() )
		{
			ImGui.Text( $"{prop.Name}: {prop.GetValue( null )}" );
		}

		ImGui.End();
	}
}
