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

	private const string manifestPath = @"Samples\mocha-minimal\project.json";

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

		// Convert args to structure so we can use the function pointers
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );

		var manifest = ProjectManifest.Load( manifestPath );
		Log.Trace( $"Loading project '{manifest.Name}'" );

		// Generate project
		var projectGenerator = new ProjectGenerator();
		var csproj = projectGenerator.GenerateProject( manifest );
		Log.Trace( $"Generated '{csproj}'" );

		var gameAssemblyInfo = new LoadedAssemblyInfo()
		{
			AssemblyName = manifest.Name,
			ProjectPath = csproj,
			SourceRoot = manifest.Resources.Code,
		};

		var editorAssemblyInfo = new LoadedAssemblyInfo()
		{
			AssemblyName = "Mocha.Editor",
			ProjectPath = "source\\Editor\\Editor.csproj",
			SourceRoot = "source\\Editor",
		};

		game = new LoadedAssemblyType<IGame>( gameAssemblyInfo );
		editor = new LoadedAssemblyType<IGame>( editorAssemblyInfo );

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
