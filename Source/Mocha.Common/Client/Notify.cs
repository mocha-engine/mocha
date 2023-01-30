namespace Mocha.Common;

public static class Notify
{
	public enum NotificationCategory
	{
		Normal,
		Error
	}

	public struct Notification
	{
		public TimeUntil Lifetime { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		public NotificationCategory Category { get; set; }

		public const float Lifespan = 5f;

		public Notification( string title, string text, NotificationCategory category = NotificationCategory.Normal )
		{
			Title = title;
			Text = text;
			Category = category;
			Lifetime = Lifespan;
		}
	}

	public static List<Notification> Notifications { get; set; } = new();

	public static void AddNotification( string title, string text, string? icon = null )
	{
		var mergedTitle = (icon == null) ? title : $"{icon} {title}";
		Notifications.Add( new Notification( mergedTitle, text ) );
	}

	public static void AddError( string title, string text, string? icon = null )
	{
		var mergedTitle = (icon == null) ? title : $"{icon} {title}";
		Notifications.Add( new Notification( mergedTitle, text, NotificationCategory.Error ) );
	}
}
