global using static Mocha.Common.Global;

using Mocha.AssetCompiler;
using Mocha.Common;
using System.Runtime.InteropServices;

namespace Mocha.Hotload;

public static class Main
{
	private static LoadedAssemblyType<IGame> game;
	private static LoadedAssemblyType<IGame> editor;

	private static bool hasInitialized;

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		game = new LoadedAssemblyType<IGame>( "build\\Mocha.Engine.dll", "source\\Engine" );
		editor = new LoadedAssemblyType<IGame>( "build\\Mocha.Editor.dll", "source\\Editor" );

		// Convert args to structure so we can use the function pointers
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );

		InitFileSystem();

		if ( !hasInitialized )
			Init();
	}

	private static void Init()
	{
		editor.Value.Startup();
		game.Value.Startup();

		hasInitialized = true;
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );

		game.Value.Update();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		Input.Update();

		game.Value.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		editor.Value.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr ptrEventName )
	{
		var eventName = Marshal.PtrToStringUTF8( ptrEventName );

		if ( eventName == null )
			return;

		Event.Run( eventName );
	}

	private static void InitFileSystem()
	{
		FileSystem.Game = new FileSystem( "content\\" );
		FileSystem.Game.AssetCompiler = new RuntimeAssetCompiler();
	}
}
