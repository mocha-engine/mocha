using ImGuiNET;

namespace Mocha.Engine;

public class Gizmos
{
	private static Texture LightTexture => Texture.Builder.FromPath( "content/icons/lightbulb.png" ).Build();

	public static void Draw()
	{
		var io = ImGui.GetIO();
		var window_flags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoBackground |
			ImGuiWindowFlags.NoMove;

		System.Numerics.Vector2 windowPos = new( 0, 0 );
		ImGui.SetNextWindowPos( windowPos, ImGuiCond.Always );

		if ( ImGui.Begin( $"##gizmo_overlay", window_flags ) )
		{
			foreach ( var ent in Entity.All )
			{
				var vp = World.Current.Camera.ViewMatrix * World.Current.Camera.ProjMatrix;
				Vector4 worldPos = new( ent.Position, 1.0f );
				var clipPos = System.Numerics.Vector4.Transform( worldPos, vp );
				var ndcPos = new Vector3( clipPos.X, clipPos.Y, clipPos.Z ) / clipPos.W;

				if ( ndcPos.Z > 1 )
					continue;

				var screenPos = new Point2(
					(int)(((ndcPos.X + 1.0) / 2.0) * Screen.Size.X),
					(int)(((-ndcPos.Y + 1.0) / 2.0) * Screen.Size.Y) );

				if ( ent is Sun )
				{
					float distance = Vector3.DistanceBetween( World.Current.Camera.Position, ent.Position ) / 16;
					distance = distance.Clamp( 0.0f, 0.1f );
					distance = 1.0f - distance;
					Vector2 size = new Vector2( 64, 64 ) * distance;

					ImGui.SetCursorPos( new System.Numerics.Vector2( screenPos.X, screenPos.Y ) - (System.Numerics.Vector2)size * 0.5f );
					EditorHelpers.Image( LightTexture, size );
				}
			}
		}

		ImGui.End();
	}
}
