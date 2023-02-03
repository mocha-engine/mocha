namespace Mocha.Editor;

public static partial class NotificationOverlay
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

		const float padding = 24.0f;
		const float margin = 8.0f;

		var viewport = ImGui.GetMainViewport();
		var workPos = viewport.WorkPos;
		var workSize = viewport.WorkSize;

		Vector2 windowPos = new(
			workPos.X + workSize.X - padding,
			workPos.Y + workSize.Y + padding
		);

		float y = 96f;

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

			t0 = EasingFunctions.InCubic( t0 );
			float t = t0.Clamp( 0, 1 );

			float xOffset = 1.0f - t;
			var windowPivot = new System.Numerics.Vector2( xOffset, 0 );

			var color = notification.Category == Notify.NotificationCategory.Error ? Theme.Red : Theme.Blue;
			color.W = alpha;

			ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 1 );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );
			ImGui.PushStyleColor( ImGuiCol.WindowBg, Theme.DarkGray );
			ImGui.SetNextWindowPos( windowPos - new Vector2( 0, y ), ImGuiCond.Always, windowPivot );
			ImGui.SetNextWindowBgAlpha( 0.0f.LerpTo( 1, alpha ) );
			ImGui.SetNextWindowSize( new System.Numerics.Vector2( 0, 0 ) );
			ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );

			if ( ImGui.Begin( $"##{notification.GetHashCode()}_overlay", windowFlags ) )
			{
				ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 1, 1, 1, alpha ) );

				ImGuiX.TextBold( notification.Title );
				ImGui.Text( notification.Text );

				ImGui.PopStyleColor();

				// Draw line on right side
				var drawList = ImGui.GetForegroundDrawList();
				var pos = ImGui.GetWindowPos();
				var size = ImGui.GetWindowSize();
				var thickness = 4f;
				var lineStart = pos + new System.Numerics.Vector2( size.X - thickness / 2f, 0 );
				var lineEnd = lineStart + new System.Numerics.Vector2( 0, size.Y );

				drawList.AddLine( lineStart, lineEnd, ImGui.GetColorU32( color ), thickness );

				ImGui.SameLine();
				ImGui.Dummy( new Vector2( thickness, 0 ) );

				var paddingRight = 32f;
				ImGui.SameLine();
				ImGui.Dummy( new Vector2( paddingRight, 0 ) );
			}

			y += ImGui.GetWindowSize().Y;
			y += margin;

			ImGui.End();

			ImGui.PopStyleColor();
			ImGui.PopStyleVar( 2 );
		}

		for ( int i = 0; i < notifications.Length; i++ )
		{
			var notification = notifications[i];

			if ( notification.Lifetime < 0 )
			{
				Common.Notify.Notifications.Remove( notification );
			}
		}
	}

	public static void Render()
	{
		DrawNotifications();
	}
}
