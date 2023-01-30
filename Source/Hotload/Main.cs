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
	private static FileSystemWatcher s_manifestWatcher = null!;
	private static TimeSince s_timeSinceLastManifestChange;

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
		var csprojPath = ReloadProjectManifest( manifestPath );

		// Setup a watcher for the project manifest.
		s_manifestWatcher = new FileSystemWatcher(
			Path.GetDirectoryName( manifestPath )!,
			Path.GetFileName( manifestPath )!)
		{
			NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size
		};

		s_manifestWatcher.Changed += OnProjectManifestChanged;
		s_manifestWatcher.EnableRaisingEvents = true;

		// Setup project assemblies.
		var gameAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = s_manifest.Name,
			ProjectPath = csprojPath,
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

		// Setup file system.
		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);
		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();

		// Start.
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

	/// <summary>
	/// Invoked when the game project manifest has changed.
	/// </summary>
	private static async void OnProjectManifestChanged( object sender, FileSystemEventArgs e )
	{
		// This will typically fire twice, so gate it with a TimeSince
		if ( s_timeSinceLastManifestChange <= 1 )
			return;

		s_timeSinceLastManifestChange = 0;
		// Wait for the program editing the file to release it.
		await Task.Delay( 10 );
		ReloadProjectManifest( e.FullPath );
	}

	/// <summary>
	/// Reloads the game project manifest.
	/// </summary>
	/// <param name="manifestPath">The absolute path to the manifest.</param>
	/// <returns>The absolute path to the generated csproj file.</returns>
	private static string ReloadProjectManifest( string manifestPath )
	{
		s_manifest = ProjectManifest.Load( manifestPath );

		var csprojPath = ProjectGenerator.Generate( s_manifest );
		return csprojPath;
	}
}
