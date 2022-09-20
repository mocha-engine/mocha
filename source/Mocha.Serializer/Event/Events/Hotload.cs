namespace Mocha.Common;

partial class Event
{
	public class HotloadAttribute : EventAttribute
	{
		public const string Name = "Event.Hotload";
		public HotloadAttribute() : base( Name ) { }
	}
}
