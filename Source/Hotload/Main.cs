using Mocha.Common;
using System.Runtime.InteropServices;

namespace Mocha;

public static class Main
{
	private static Game game;
	private static World world;

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		Mocha.Common.Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );

		game = new();
		game.Startup();
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		game.Update();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		game.Render();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		game.DrawEditor();
	}

	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr ptrEventName )
	{
		var eventName = Marshal.PtrToStringUTF8( ptrEventName );

		if ( eventName == null )
			return;

		Event.Run( eventName );
	}
}
