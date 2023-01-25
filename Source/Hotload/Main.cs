global using static Mocha.Common.Global;
using Mocha.Common;
using MochaTool.AssetCompiler;
using System.Runtime.InteropServices;

namespace Mocha.Hotload;

public static class Main
{
	private static ProjectAssembly<IGame> game;
	private static ProjectAssembly<IGame> editor;

	private static bool hasInitialized;

	private const string manifestPath = @"Samples\mocha-minimal\project.json";

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		// This MUST be done before everything
		Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

		// Convert args to structure so we can use the function pointers.
		// This MUST be done before calling any native functions
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );

		// Initialize upgrader, we do this as early as possible to prevent
		// slowdowns while the engine is running.
		Upgrader.Init();

		// Get the current loaded project from native
		var manifestPath = Glue.Engine.GetProjectPath();
		var manifest = ProjectManifest.Load( manifestPath );
		Log.Trace( $"Loading project '{manifest.Name}'" );

		// Generate project
		var projectGenerator = new ProjectGenerator();
		var csproj = projectGenerator.GenerateProject( manifest );
		Log.Trace( $"Generated '{csproj}'" );

		var gameAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = manifest.Name,
			ProjectPath = csproj,
			SourceRoot = manifest.Resources.Code,
		};

		var editorAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = "Mocha.Editor",
			ProjectPath = "source\\Editor\\Editor.csproj",
			SourceRoot = "source\\Editor",
		};

		game = new ProjectAssembly<IGame>( gameAssemblyInfo );
		editor = new ProjectAssembly<IGame>( editorAssemblyInfo );

		InitFileSystem();

		if ( !hasInitialized )
			Init();
	}

	private static void Init()
	{
		editor.Value?.Startup();
		game.Value?.Startup();

		hasInitialized = true;
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );

		game.Value?.Update();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		Time.UpdateFrom( Glue.Engine.GetDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		Input.Update();

		game.Value?.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		if ( game == null )
			throw new Exception( "Invoke Run() first" );

		editor.Value?.FrameUpdate();
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
