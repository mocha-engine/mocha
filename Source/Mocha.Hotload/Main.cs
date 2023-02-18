global using static Mocha.Common.Global;
using Mocha.Common;
using Mocha.Common.Console;
using MochaTool.AssetCompiler;
using System.Runtime.InteropServices;

namespace Mocha.Hotload;

public static class Main
{
	private static ProjectAssembly<IGame> s_editor = null!;

	private static ProjectAssembly<IGame> s_client = null!;
	private static ProjectAssembly<IGame> s_server = null!;

	private static ProjectManifest s_manifest;
	private static FileSystemWatcher s_manifestWatcher = null!;
	private static TimeSince s_timeSinceLastManifestChange;

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		// This MUST be done before everything
		if ( !Microsoft.Build.Locator.MSBuildLocator.IsRegistered )
			Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

		// Convert args to structure so we can use the function pointers.
		// This MUST be done before calling any native functions
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		NativeEngine = new Glue.Root();
		NativeEngine.NativePtr = Global.UnmanagedArgs.__Root;

		// Initialize the logger
		Log = new NativeLogger();

		// We change some behaviour if we're on a dedicated server:
		// - We don't compile for the client
		// - We don't run the game in a "client" context
		// - We don't render or draw anything
		bool isDedicatedServer = NativeEngine.IsDedicatedServer();

		// TODO: Is there a better way to register these cvars?
		// Register cvars for assemblies that will never hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Hotload.Main ).Assembly );  // Hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.Common.IGame ).Assembly );  // Common
		ConsoleSystem.Internal.RegisterAssembly( typeof( Mocha.BaseGame ).Assembly );      // Engine

		// Initialize upgrader, we do this as early as possible to prevent
		// slowdowns while the engine is running.
		Upgrader.Init();

		// Get the current loaded project from native
		var manifestPath = NativeEngine.GetProjectPath();
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
		var serverAssemblyInfo = new ProjectAssemblyInfo()
		{
			AssemblyName = s_manifest.Name,
			ProjectPath = csprojPath,
			SourceRoot = s_manifest.Resources.Code,
			IsServer = true
		};

		var clientAssemblyInfo = new ProjectAssemblyInfo()
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

		if ( isDedicatedServer )
		{
			// TODO: Listen server logic
			s_server = new ProjectAssembly<IGame>( serverAssemblyInfo );
		}

		if ( !isDedicatedServer )
		{
			s_client = new ProjectAssembly<IGame>( clientAssemblyInfo );
			s_editor = new ProjectAssembly<IGame>( editorAssemblyInfo );
		}

		// Setup file system.
		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);
		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();

		// Start.
		SetServerContext( true );
		s_server?.EntryPoint.Startup();

		SetServerContext( false );
		s_editor?.EntryPoint.Startup();
		s_client?.EntryPoint.Startup();
	}

	private static void SetServerContext( bool isServer )
	{
		Core.IsServer = isServer;
		Core.IsClient = !isServer;
	}

	[UnmanagedCallersOnly]
	public static void Update()
	{
		Time.UpdateFrom( NativeEngine.GetTickDeltaTime() );

		if ( s_client != null )
		{
			SetServerContext( false );
			s_client?.EntryPoint.Update();

			Event.Run( s_client!.Assembly, Event.TickAttribute.Name );
		}

		if ( s_server != null )
		{
			SetServerContext( true );
			s_server?.EntryPoint.Update();

			Event.Run( s_server!.Assembly, Event.TickAttribute.Name );
		}
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		Time.UpdateFrom( NativeEngine.GetFrameDeltaTime() );
		Screen.UpdateFrom( NativeEngine.GetRenderSize() );
		Input.Update();

		SetServerContext( false );
		s_client?.EntryPoint.FrameUpdate();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		SetServerContext( false );
		s_editor?.EntryPoint.FrameUpdate();
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

	[UnmanagedCallersOnly]
	public static void DispatchIntCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<IntCVarDispatchInfo>( infoPtr );
		string? name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
	}

	[UnmanagedCallersOnly]
	public static void InvokeCallback( IntPtr handlePtr )
	{
		uint handle = (uint)handlePtr.ToInt64();
		CallbackDispatcher.Invoke( handle );
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

		SetGlobals();

		var csprojPath = ProjectGenerator.Generate( s_manifest );
		return csprojPath;
	}

	/// <summary>
	/// Set the values in the <see cref="Project"/> global class so that
	/// developers can use values like tick rate, etc. in their games
	/// </summary>
	private static void SetGlobals()
	{
		Core.TickRate = s_manifest.Properties.TickRate;
	}
}
