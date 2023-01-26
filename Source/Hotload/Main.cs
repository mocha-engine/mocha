global using static Mocha.Common.Global;
using Mocha.Common;
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

		// Initialize upgrader, we do this as early as possible to prevent
		// slowdowns while the engine is running.
		Upgrader.Init();

		// Get the current loaded project from native
		var manifestPath = Glue.Engine.GetProjectPath();
		s_manifest = ProjectManifest.Load( manifestPath );
		Log.Trace( $"Loading project '{s_manifest.Name}'" );

		// Generate project
		var projectGenerator = new ProjectGenerator();
		var csproj = projectGenerator.GenerateProject( s_manifest );
		Log.Trace( $"Generated '{csproj}'" );

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
			"content\\core",
			s_manifest.Resources.Content );

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

		Time.UpdateFrom( Glue.Engine.GetDeltaTime() );
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
}
