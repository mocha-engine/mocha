using System.Reflection;

namespace Mocha;

public static partial class Event
{
	struct EventRef
	{
		public string Name { get; set; }
		public MethodInfo Method { get; set; }
		public object Object { get; set; }

		public EventRef( string name, MethodInfo method, object obj )
		{
			Name = name;
			Method = method;
			Object = obj;
		}
	}

	private static List<EventRef> events = new();

	public static void Register( object obj )
	{
		var attributes = obj.GetType().GetMethods()
			.Where( m => m.GetCustomAttribute<EventAttribute>() != null )
			.Select( m => new EventRef( m.GetCustomAttribute<EventAttribute>().EventName, m, obj ) );

		events.AddRange( attributes );
	}

	public static void RegisterStatics()
	{
		foreach ( var type in Assembly.GetExecutingAssembly().GetTypes() )
		{
			var attributes = type.GetMethods()
				.Where( m => m.GetCustomAttribute<EventAttribute>() != null && m.IsStatic )
				.Select( m => new EventRef( m.GetCustomAttribute<EventAttribute>().EventName, m, null ) );
		}
	}

	public static void Run( string name, params object[] parameters )
	{
		events.ForEach( e =>
		{
			if ( e.Name == name )
				e.Method?.Invoke( e.Object, parameters );
		} );
	}

	public static void Run( string name )
	{
		events.ForEach( e =>
		{
			if ( e.Name == name )
				e.Method?.Invoke( e.Object, null );
		} );
	}
}
