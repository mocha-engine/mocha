global using static Mocha.Common.Global;
using Mocha.Common;
using Mocha.Common.Console;
using MochaTool.AssetCompiler;
using System.Runtime.InteropServices;

namespace Mocha.Hotload;

public static class Main
{
	private static ProjectAssembly<IGame> s_game;
	private static ProjectAssembly<IGame> s_editor;

	private static bool s_hasInitialized;
	private static ProjectManifest s_manifest;

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		// This MUST be done before everything
		Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

		// Convert args to structure so we can use the function pointers.
		// This MUST be done before calling any native functions
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );

		// Initialize the logger
		Log = new NativeLogger();

		// TODO: Is there a better way to register these cvars?
		// Register cvars for assemblies that will never hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Hotload.Main ).Assembly );	// Hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Common.IGame ).Assembly );	// Common
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.BaseGame ).Assembly );		// Engine

		// Initialize upgrader, we do this as early as possible to prevent
		// slowdowns while the engine is running.
		Upgrader.Init();

		// Get the current loaded project from native
		var manifestPath = Glue.Engine.GetProjectPath();
		s_manifest = ProjectManifest.Load( manifestPath );

		// Generate project
		var projectGenerator = new ProjectGenerator();
		var csproj = projectGenerator.GenerateProject( s_manifest );

		var gameAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = s_manifest.Name,
			ProjectPath = csproj,
			SourceRoot = s_manifest.Resources.Code,
		};

		var editorAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = "Mocha.Editor",
			ProjectPath = "source\\Editor\\Editor.csproj",
			SourceRoot = "source\\Editor",
		};

		s_game = new ProjectAssembly<IGame>( gameAssemblyInfo );
		s_editor = new ProjectAssembly<IGame>( editorAssemblyInfo );

		InitFileSystem();

		if ( !s_hasInitialized )
			Init();
	}

	private static void InitFileSystem()
	{
		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);

		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();
	}

	private static void Init()
	{
		s_editor.Value?.Startup();
		s_game.Value?.Startup();

		s_hasInitialized = true;
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		if ( s_game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );

		s_game.Value?.Update();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		if ( s_game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		Input.Update();

		s_game.Value?.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		if ( s_game == null )
			throw new Exception( "Invoke Run() first" );

		s_editor.Value?.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr ptrEventName )
	{
		var eventName = Marshal.PtrToStringUTF8( ptrEventName );

		if ( eventName == null )
			return;

		Event.Run( eventName );
	}

	[UnmanagedCallersOnly]
	public static void DispatchCommand( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<ConCmdDispatchInfo>( infoPtr );
		var name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		var arguments = new List<string>();

		if ( info.size != 0 )
		{
			// TODO: Remove this alloc
			var stringPtrs = new IntPtr[info.size];
			Marshal.Copy( info.data, stringPtrs, 0, info.size );

			arguments.Capacity = info.size;

			for ( int i = 0; i < info.size; i++ )
			{
				arguments.Add( Marshal.PtrToStringUTF8( stringPtrs[i] ) ?? "" );
			}
		}

		Log.Trace( $"Dispatching managed command '{name}'" );

		ConsoleSystem.Internal.DispatchCommand( name, arguments );
	}

	[UnmanagedCallersOnly]
	public static void DispatchStringCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<StringCVarDispatchInfo>( infoPtr );
	}

	[UnmanagedCallersOnly]
	public static void DispatchFloatCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<FloatCVarDispatchInfo>( infoPtr );
	}

	[UnmanagedCallersOnly]
	public static void DispatchBoolCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<BoolCVarDispatchInfo>( infoPtr );
	}
}
