namespace Mocha.Common;

partial class Event
{
	public class TickAttribute : EventAttribute
	{
		public const string Name = "Event.Tick";
		public TickAttribute() : base( Name ) { }
	}
}
