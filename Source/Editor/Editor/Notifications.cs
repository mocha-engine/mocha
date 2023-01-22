namespace Mocha.Editor;

public static partial class Notifications
{
	private static void DrawNotifications()
	{
		var windowFlags = ImGuiWindowFlags.NoDecoration |
			ImGuiWindowFlags.AlwaysAutoResize |
			ImGuiWindowFlags.NoSavedSettings |
			ImGuiWindowFlags.NoFocusOnAppearing |
			ImGuiWindowFlags.NoNav |
			ImGuiWindowFlags.NoInputs |
			ImGuiWindowFlags.NoDocking |
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
			if ( notification.Lifetime < 0 )
				continue;

			float transitionTime = 0.5f;
			float t0 = notification.Lifetime.Until.LerpInverse( Notify.Notification.Lifespan - transitionTime, Notify.Notification.Lifespan );
			float t1 = notification.Lifetime.Until.LerpInverse( transitionTime, 0.0f );
			float alpha = 1.0f - (t0 + t1).Clamp( 0, 1 );

			t0 = EasingFunctions.InBounce( t0 );
			float t = t0.Clamp( 0, 1 );

			float xOffset = 1.0f - t;
			var windowPivot = new System.Numerics.Vector2( xOffset, 0 );

			ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 1 );
			ImGui.SetNextWindowPos( windowPos + new System.Numerics.Vector2( 0, y ), ImGuiCond.Always, windowPivot );
			ImGui.SetNextWindowBgAlpha( 0.0f.LerpTo( 1, alpha ) );
			ImGui.SetNextWindowSize( new System.Numerics.Vector2( 0, 0 ) );
			ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );

			if ( ImGui.Begin( $"##{notification.GetHashCode()}_overlay", windowFlags ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 1, 1, 1, alpha ) );

				ImGuiX.TextBold( notification.Title );
				ImGui.Text( notification.Text );

				ImGui.PopStyleColor();

				y += ImGui.GetWindowSize().Y + 8;
			}

			ImGui.End();

			ImGui.PopStyleVar();
		}
	}

	public static void Render()
	{
		DrawNotifications();
	}
}
