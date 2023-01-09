using Mocha.AssetCompiler;
using System.Runtime.InteropServices;
using VConsoleLib;

namespace Mocha;

public class Main
{
	private static World world;
	private static VConsoleServer vconsoleServer;

	private static void InitFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
	}

	private static void InitFileSystem()
	{
		FileSystem.Game = new FileSystem( "content\\" );
		FileSystem.Game.AssetCompiler = new RuntimeAssetCompiler();
	}

	private static void InitVConsole()
	{
		vconsoleServer = new();
		Log.OnLog += ( s ) => vconsoleServer.Log( s );

		vconsoleServer.OnCommand += ( s ) =>
		{
			Log.Info( $"Command: {s}" );
		};
	}

	private static void InitImGui()
	{
		ImGuiNative.igSetCurrentContext( Glue.Editor.GetContextPointer() );
	}

	private static void InitGame()
	{
		world = new World();
	}

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		InitFunctionPointers( args );
		InitFileSystem();
		InitVConsole();
		InitImGui();
		InitGame();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		Input.Update();
		Time.UpdateFrom( Glue.Entities.GetDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );

		// HACK: bail if deltatime is too high. This usually happens when the window
		// is touched, but can happen with framerate spikes.
		// Need to eventually look at https://gafferongames.com/post/fix_your_timestep/
		if ( Time.Delta > 0.1f )
			return;

		world.Update();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		Editor.Editor.Draw();
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
