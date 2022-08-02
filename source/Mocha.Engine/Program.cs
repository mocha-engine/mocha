global using static Mocha.Engine.Global;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;
using System.Runtime.InteropServices;

namespace Mocha.Engine;

/// <summary>
/// Program entry point
/// </summary>
public class Program
{
	[StructLayout( LayoutKind.Sequential )]
	public struct UnmanagedArgs
	{
		public IntPtr CLoggerPtr;

		public IntPtr CreateMethodPtr;
		public IntPtr DeleteMethodPtr;
		public IntPtr LogMethodPtr;
		public IntPtr InteropTestMethodPtr;
	}

	public class CLogger
	{
		private IntPtr instance;
		private delegate IntPtr CreateDelegate();
		private CreateDelegate CreateMethod;

		private delegate void DeleteDelegate( IntPtr instance );
		private DeleteDelegate DeleteMethod;

		private delegate void LogDelegate( IntPtr instance );
		private LogDelegate LogMethod;

		private delegate IntPtr InteropTestDelegate( IntPtr instance, [MarshalAs( UnmanagedType.LPStr )] string a, [MarshalAs( UnmanagedType.LPStr )] string b );
		private InteropTestDelegate InteropTestMethod;

		public CLogger( UnmanagedArgs args )
		{
			this.instance = args.CLoggerPtr;
			this.CreateMethod = Marshal.GetDelegateForFunctionPointer<CreateDelegate>( args.CreateMethodPtr );
			this.DeleteMethod = Marshal.GetDelegateForFunctionPointer<DeleteDelegate>( args.DeleteMethodPtr );
			this.LogMethod = Marshal.GetDelegateForFunctionPointer<LogDelegate>( args.LogMethodPtr );
			this.InteropTestMethod = Marshal.GetDelegateForFunctionPointer<InteropTestDelegate>( args.InteropTestMethodPtr );
		}

		public IntPtr Create()
		{
			return this.CreateMethod();
		}

		public void Delete()
		{
			this.DeleteMethod( instance );
		}

		public void Log()
		{
			this.LogMethod( instance );
		}

		public IntPtr InteropTest( [MarshalAs( UnmanagedType.LPStr )] string a, [MarshalAs( UnmanagedType.LPStr )] string b )
		{
			return this.InteropTestMethod( instance, a, b );
		}
	}

	//[UnmanagedCallersOnly]
	//public static void HostedMain( IntPtr args )
	//{
	//	var unmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
	//	var logger = new CLogger( unmanagedArgs );

	//	logger.Log( "HELLO :D" );

	//	for ( int i = 0; i < 3; ++i )
	//	{
	//		logger.Log( $"Hello! {i}" );
	//	}
	//}

	public static void Main( string[] args )
	{
		var game = new Game();
	}

	private void RunGameAndHandleExceptions()
	{
		try
		{
			Main( new string[0] );
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"Caught unhandled exception:\n{ex}" );
		}
	}
}
