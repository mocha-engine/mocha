namespace Mocha.Common;

partial class Event
{
	public class Game
	{
		public class HotloadAttribute : EventAttribute
		{
			public const string Name = "Event.Game.Hotload";
			public HotloadAttribute() : base( Name ) { }
		}

		public class LoadAttribute : EventAttribute
		{
			public const string Name = "Event.Game.Load";
			public LoadAttribute() : base( Name ) { }
		}
	}
}
