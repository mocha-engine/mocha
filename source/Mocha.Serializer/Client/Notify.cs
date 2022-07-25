namespace Mocha.Common;

public static class Notify
{
	public struct Notification
	{
		public TimeSince Lifetime { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }

		public Notification( string title, string text )
		{
			this.Title = title;
			this.Text = text;

			// TODO: What the fuck
			this.Lifetime = -0.29f;
		}
	}

	public static List<Notification> Notifications { get; set; } = new();

	public static void AddNotification( string title, string text )
	{
		Notifications.Add( new Notification( title, text ) );
	}
}
