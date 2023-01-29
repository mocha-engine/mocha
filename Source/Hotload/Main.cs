global using static Mocha.Common.Global;
using Mocha.Common;
using MochaTool.AssetCompiler;
using System.Runtime.InteropServices;

namespace Mocha.Hotload;

public static class Main
{
	private static ProjectAssembly<IGame> s_game = null!;
	private static ProjectAssembly<IGame> s_editor = null!;

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

		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);
		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();

		s_editor.EntryPoint.Startup();
		s_game.EntryPoint.Startup();
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );

		s_game.EntryPoint.Update();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		Time.UpdateFrom( Glue.Engine.GetTickDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetRenderSize() );
		Input.Update();

		s_game.EntryPoint.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		s_editor.EntryPoint.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr ptrEventName )
	{
		var eventName = Marshal.PtrToStringUTF8( ptrEventName );
		if ( eventName is null )
			return;

		Event.Run( eventName );
	}
}
