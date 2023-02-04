global using static Mocha.Common.Global;
using Mocha.Common;
using Mocha.Common.Console;
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

		// TODO: Is there a better way to register these cvars?
		// Register cvars for assemblies that will never hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Hotload.Main ).Assembly );   // Hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Common.IGame ).Assembly );   // Common
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.BaseGame ).Assembly );       // Engine

		// Initialize upgrader, we do this as early as possible to prevent
		// slowdowns while the engine is running.
		Upgrader.Init();

		// Get the current loaded project from native
		var manifestPath = Glue.Engine.GetProjectPath();
		var csprojPath = ReloadProjectManifest( manifestPath );

		// Setup a watcher for the project manifest.
		s_manifestWatcher = new FileSystemWatcher(
			Path.GetDirectoryName( manifestPath )!,
			Path.GetFileName( manifestPath )! )
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
			ProjectPath = "source\\Mocha.Editor\\Mocha.Editor.csproj",
			SourceRoot = "source\\Mocha.Editor",
		};

		s_game = new ProjectAssembly<IGame>( gameAssemblyInfo );

		if ( Host.IsClient )
			s_editor = new ProjectAssembly<IGame>( editorAssemblyInfo );

		// Setup file system.
		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);
		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();

		// Start.
		if ( Host.IsClient )
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
		if ( !Host.IsClient )
			return;

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

	[UnmanagedCallersOnly]
	public static void DispatchCommand( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<ConCmdDispatchInfo>( infoPtr );
		string? name = Marshal.PtrToStringUTF8( info.name );

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

		ConsoleSystem.Internal.DispatchCommand( name, arguments );
	}

	[UnmanagedCallersOnly]
	public static void DispatchStringCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<StringCVarDispatchInfo>( infoPtr );
		string? name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		string oldValue = Marshal.PtrToStringUTF8( info.oldValue ) ?? "";
		string newValue = Marshal.PtrToStringUTF8( info.newValue ) ?? "";

		ConsoleSystem.Internal.DispatchConVarCallback( name, oldValue, newValue );
	}

	[UnmanagedCallersOnly]
	public static void DispatchFloatCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<FloatCVarDispatchInfo>( infoPtr );
		string? name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
	}

	[UnmanagedCallersOnly]
	public static void DispatchBoolCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<BoolCVarDispatchInfo>( infoPtr );
		string? name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
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
