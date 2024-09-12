global using static Mocha.Common.Global;
using Mocha.Common;
using Mocha.Common.Console;
using Mocha.Hotload.Projects;
using Mocha.Hotload.Upgrading;
using Mocha.Hotload.Util;
using MochaTool.AssetCompiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Mocha.Hotload;

/// <summary>
/// Contains all of the functionality to bootstrap the C# land.
/// </summary>
public static class Main
{
	/// <summary>
	/// The assembly wrapper that represents the Mocha editor.
	/// </summary>
	private static ProjectAssembly<IGame> s_editor = null!;

	/// <summary>
	/// The assembly wrapper that represents the client side of the loaded project.
	/// </summary>
	private static ProjectAssembly<IGame> s_client = null!;
	/// <summary>
	/// The assembly wrapper that represents the server side of the loaded project.
	/// </summary>
	private static ProjectAssembly<IGame> s_server = null!;

	/// <summary>
	/// The loaded project manifest.
	/// </summary>
	private static ProjectManifest s_manifest;
	/// <summary>
	/// The <see cref="FileSystemWatcher"/> responsible of checking when the manifest is changed on disk.
	/// </summary>
	private static FileSystemWatcher s_manifestWatcher = null!;
	/// <summary>
	/// The time since the last change happened to the manifest on disk.
	/// </summary>
	private static TimeSince s_timeSinceLastManifestChange;

	/// <summary>
	/// Bootstraps the C# land.
	/// </summary>
	/// <param name="args">The pointer to the <see cref="UnmanagedArgs"/> for interoperability.</param>
	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
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
		ConsoleSystem.Internal.RegisterAssembly( typeof( Main ).Assembly );         // Hotload
		ConsoleSystem.Internal.RegisterAssembly( typeof( IGame ).Assembly );        // Common
		ConsoleSystem.Internal.RegisterAssembly( typeof( BaseGame ).Assembly );     // Engine

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

		// Setup file system.
		FileSystem.Mounted = new FileSystem(
			s_manifest.Resources.Content,
			"content\\core"
		);
		FileSystem.Mounted.AssetCompiler = new RuntimeAssetCompiler();

		// Create assemblies.
		if ( isDedicatedServer )
		{
			SetServerContext( true );

			// TODO: Listen server logic
			s_server = new ProjectAssembly<IGame>( serverAssemblyInfo );

			// Start.
			s_server.EntryPoint?.Startup();
		}
		else
		{
			SetServerContext( false );

			s_editor = new ProjectAssembly<IGame>( editorAssemblyInfo );
			// The editor should never fail to compile.
			Debug.Assert( s_editor.EntryPoint is not null );
			s_client = new ProjectAssembly<IGame>( clientAssemblyInfo );

			// Start.
			s_editor.EntryPoint.Startup();
			s_client.EntryPoint?.Startup();
		}
	}

	/// <summary>
	/// Update loop for the C# land.
	/// </summary>
	[UnmanagedCallersOnly]
	public static void Update()
	{
		// Update time.
		Time.UpdateFrom( NativeEngine.GetTickDeltaTime() );

		// Update client realm.
		if ( s_client is not null )
		{
			SetServerContext( false );
			s_client.EntryPoint?.Update();

			if ( s_client.Assembly is not null )
				Event.Run( s_client.Assembly, Event.TickAttribute.Name );
		}

		// Update server realm.
		if ( s_server is not null )
		{
			SetServerContext( true );
			s_server.EntryPoint?.Update();

			if ( s_server.Assembly is not null )
				Event.Run( s_server.Assembly, Event.TickAttribute.Name );
		}
	}

	/// <summary>
	/// Render loop for the C# land.
	/// </summary>
	[UnmanagedCallersOnly]
	public static void Render()
	{
		// Update.
		Time.UpdateFrom( NativeEngine.GetFrameDeltaTime() );
		Screen.UpdateFrom( NativeEngine.GetRenderSize() );
		Input.Update();

		// Render client.
		SetServerContext( false );
		s_client.EntryPoint?.FrameUpdate();
	}

	/// <summary>
	/// Drawing editor loop for the C# land.
	/// </summary>
	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		// Render editor.
		SetServerContext( false );
		s_editor.EntryPoint.FrameUpdate();
	}

	/// <summary>
	/// Fires an event that came from unmanaged.
	/// </summary>
	/// <param name="ptrEventName">The pointer to the UTF8 string that contains the event name.</param>
	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr ptrEventName )
	{
		var eventName = Marshal.PtrToStringUTF8( ptrEventName );
		if ( eventName is null )
			return;

		Event.Run( eventName );
	}

	/// <summary>
	/// Dispatches a console command that came from unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="ConCmdDispatchInfo"/> struct.</param>
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
				arguments.Add( Marshal.PtrToStringUTF8( stringPtrs[i] ) ?? "" );
		}

		ConsoleSystem.Internal.DispatchCommand( name, arguments );
	}

	/// <summary>
	/// Fires when a string console variable is changed in unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="StringCVarDispatchInfo"/> struct.</param>
	[UnmanagedCallersOnly]
	public static void DispatchStringCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<StringCVarDispatchInfo>( infoPtr );
		var name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		var oldValue = Marshal.PtrToStringUTF8( info.oldValue ) ?? "";
		var newValue = Marshal.PtrToStringUTF8( info.newValue ) ?? "";

		ConsoleSystem.Internal.DispatchConVarCallback( name, oldValue, newValue );
	}

	/// <summary>
	/// Fires when a float console variable is changed in unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="FloatCVarDispatchInfo"/> struct.</param>
	[UnmanagedCallersOnly]
	public static void DispatchFloatCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<FloatCVarDispatchInfo>( infoPtr );
		var name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
	}

	/// <summary>
	/// Fires when a boolean console variable is changed in unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="BoolCVarDispatchInfo"/> struct.</param>
	[UnmanagedCallersOnly]
	public static void DispatchBoolCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<BoolCVarDispatchInfo>( infoPtr );
		var name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
	}

	/// <summary>
	/// Fires when a integer console variable is changed in unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="IntCVarDispatchInfo"/> struct.</param>
	[UnmanagedCallersOnly]
	public static void DispatchIntCVarCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<IntCVarDispatchInfo>( infoPtr );
		var name = Marshal.PtrToStringUTF8( info.name );

		if ( name is null )
			return;

		ConsoleSystem.Internal.DispatchConVarCallback( name, info.oldValue, info.newValue );
	}

	/// <summary>
	/// Fired when a callback has been triggered in unmanaged code.
	/// </summary>
	/// <param name="infoPtr">The pointer to the <see cref="ManagedCallbackDispatchInfo"/> struct.</param>
	[UnmanagedCallersOnly]
	public static void InvokeCallback( IntPtr infoPtr )
	{
		var info = Marshal.PtrToStructure<ManagedCallbackDispatchInfo>( infoPtr );

		if ( info.argsSize > 0 )
			CallbackDispatcher.Invoke( info.handle, info.args );
		else
			CallbackDispatcher.Invoke( info.handle );
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
		while ( FileUtil.IsFileInUse( e.FullPath ) )
			await Task.Delay( 1 );

		ReloadProjectManifest( e.FullPath );
	}

	/// <summary>
	/// Sets the current realm context of the application.
	/// </summary>
	/// <param name="isServer">Whether or not the new context is the server.</param>
	private static void SetServerContext( bool isServer )
	{
		Core.IsServer = isServer;
		Core.IsClient = !isServer;
	}

	/// <summary>
	/// Reloads the game project manifest.
	/// </summary>
	/// <param name="manifestPath">The absolute path to the manifest.</param>
	/// <returns>A task that represents the asynchronous operation. The tasks return value is the absolute path to the generated csproj file.</returns>
	private static string ReloadProjectManifest( string manifestPath )
	{
		s_manifest = ProjectManifest.Load( manifestPath );
		SetGlobals();

		var csprojPath = Path.Combine( s_manifest.Resources.Code, "code.csproj" );
		var csprojDocument = CSharpProject.FromManifest( s_manifest ).ToXml();

		// Write csproj to disk.
		var stream = File.Open( csprojPath, FileMode.Create );
		var writer = new XmlTextWriter( stream, Encoding.UTF8 )
		{
			Formatting = Formatting.Indented,
		};
		csprojDocument.WriteContentTo( writer );
		writer.Flush();
		writer.Close();

		// Write launch settings for csproj to disk.
		var propertiesDir = Path.Combine( Path.GetDirectoryName( s_manifest.Resources.Code )!, "Properties" );
		if ( !Directory.Exists( propertiesDir ) )
			Directory.CreateDirectory( propertiesDir );

		var relativeManifestPath = Path.GetRelativePath( Environment.CurrentDirectory, manifestPath ).Replace( "\\", "\\\\" );
		var launchSettings = LaunchSettingsText
			.Replace( "%__CUR_DIR__", Environment.CurrentDirectory )
			.Replace( "%__REL_MANIFEST_PATH__", relativeManifestPath );
		File.WriteAllText( propertiesDir + "\\launchSettings.json", launchSettings );

		return csprojPath;
	}

	/// <summary>
	/// Set the values in the <see cref="Core"/> global class so that developers can use values like tick rate, etc. in their games.
	/// </summary>
	private static void SetGlobals()
	{
		Core.TickRate = s_manifest.Properties.TickRate;
	}

	/// <summary>
	/// The JSON to write into each projects launch settings.
	/// </summary>
	private const string LaunchSettingsText = """
	{
		"profiles": {
			"Mocha": {
				"commandName": "Executable",
				"executablePath": "%__CUR_DIR__\\build\\Mocha.exe",
				"commandLineArgs": "-project %__REL_MANIFEST_PATH__"
				"workingDirectory": "%__CUR_DIR__",
				"nativeDebugging": true
			},
			"Mocha Dedicated Server": {
				"commandName": "Executable",
				"executablePath": "%__CUR_DIR__\\build\\MochaDedicatedServer.exe",
				"commandLineArgs": "-project %__REL_MANIFEST_PATH__"
				"workingDirectory": "%__CUR_DIR__",
				"nativeDebugging": true
			}
		}
	}
	""";
}
