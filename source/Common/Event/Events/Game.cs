namespace Mocha.Common;

partial class Event
{
	public class Game
	{
		public class HotloadAttribute : EventAttribute
		{
			public const string Name = "Event.Game.Load";
			public HotloadAttribute() : base( Name ) { }
		}
	}
}
