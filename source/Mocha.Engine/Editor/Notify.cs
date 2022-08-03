using ImGuiNET;

namespace Mocha.Engine;

public static class Notify
{
	public static void Draw()
	{
		var windowFlags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoMove;

		const float padding = 8.0f;

		var viewport = ImGui.GetMainViewport();
		var workPos = viewport.WorkPos;
		var workSize = viewport.WorkSize;

		System.Numerics.Vector2 windowPos;

		windowPos.X = workPos.X + workSize.X - padding;
		windowPos.Y = workPos.Y + padding;

		float y = 0;

		var notifications = Common.Notify.Notifications.ToArray();
		for ( int i = 0; i < notifications.Length; i++ )
		{
			var notification = notifications[i];
			if ( notification.Lifetime > 5 )
				continue;

			float t0 = notification.Lifetime.Relative.LerpInverse( 0.5f, 0.0f );
			float t1 = notification.Lifetime.Relative.LerpInverse( 4.5f, 5.0f );
			float alpha = 1.0f - (t0 + t1).Clamp( 0, 1 );

			t0 = EasingFunctions.InExpo( t0 );
			float t = t0.Clamp( 0, 1 );

			float xOffset = 1.0f - t;
			var windowPivot = new System.Numerics.Vector2( xOffset, 0 );

			ImGui.PushStyleColor( ImGuiCol.WindowBg, new Vector4( 0, 0, 0, 1 ) );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 1 );
			ImGui.SetNextWindowPos( windowPos + new System.Numerics.Vector2( 0, y ), ImGuiCond.Always, windowPivot );
			ImGui.SetNextWindowBgAlpha( 0.0f.LerpTo( 1, alpha ) );
			ImGui.SetNextWindowSize( new System.Numerics.Vector2( 0, 0 ) );

			if ( ImGui.Begin( $"##{notification.GetHashCode()}_overlay", windowFlags ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 1, 1, 1, alpha ) );

				ImGui.PushFont( Editor.BoldFont );
				ImGui.Text( notification.Title );
				ImGui.PopFont();

				ImGui.Text( notification.Text );

				y += ImGui.GetWindowSize().Y + 8;
				ImGui.End();

				ImGui.PopStyleColor();
			}

			ImGui.PopStyleVar( 1 );
			ImGui.PopStyleColor();
		}
	}
}
