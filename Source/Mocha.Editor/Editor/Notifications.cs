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

		const float padding = 24.0f;
		const float margin = 8.0f;

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

			t0 = EasingFunctions.InCubic( t0 );
			float t = t0.Clamp( 0, 1 );

			float xOffset = 1.0f - t;
			var windowPivot = new System.Numerics.Vector2( xOffset, 0 );

			ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 1 );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );
			ImGui.SetNextWindowPos( windowPos + new System.Numerics.Vector2( 0, y ), ImGuiCond.Always, windowPivot );
			ImGui.SetNextWindowBgAlpha( 0.0f.LerpTo( 1, alpha ) );
			ImGui.SetNextWindowSize( new System.Numerics.Vector2( 0, 0 ) );
			ImGui.SetNextWindowViewport( ImGui.GetMainViewport().ID );

			if ( ImGui.Begin( $"##notification_{i}_overlay", windowFlags ) )
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

				var color = notification.Category == Notify.NotificationCategory.Error ? Theme.Red : Theme.Blue;
				color.W = alpha;

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
