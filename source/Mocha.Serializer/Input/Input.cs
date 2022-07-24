using System.Runtime.CompilerServices;
using System.Text;
using Veldrid.Sdl2;

namespace Mocha.Common;

// TODO: Decouple from SDL
public static partial class Input
{
	public enum MouseModes
	{
		Locked,
		Unlocked
	}

	public static MouseModes MouseMode { get; set; }

	public static MochaInputSnapshot Snapshot { get; private set; }

	public static System.Numerics.Vector2 MouseDelta => Snapshot.MouseDelta;
	public static System.Numerics.Vector2 MousePosition => Snapshot.MousePosition;

	public static float Forward => Snapshot.Forward;
	public static float Left => Snapshot.Left;
	public static float Up => Snapshot.Up;

	public static bool MouseLeft => Snapshot.MouseLeft;
	public static bool MouseRight => Snapshot.MouseRight;

	public static unsafe void Update()
	{
		if ( Sdl2Native.SDL_SetRelativeMouseMode( MouseMode == MouseModes.Locked ) != 0 )
			throw new Exception();

		var lastKeysDown = Snapshot?.KeysDown.ToList() ?? new();
		var lastKeyEvents = Snapshot?.KeyEvents.ToList() ?? new();
		var lastMouseEvents = Snapshot?.MouseEvents.ToList() ?? new();
		Snapshot ??= new();
		Snapshot.KeysDown = lastKeysDown.ToList();
		Snapshot.LastKeysDown = lastKeysDown;
		Snapshot.MouseDelta = Vector2.Zero;
		Snapshot.WheelDelta = 0;

		Sdl2Native.SDL_PumpEvents();

		List<Veldrid.KeyEvent> veldridKeyEvents = new( lastKeyEvents );
		List<Veldrid.MouseEvent> veldridMouseEvents = new( lastMouseEvents );
		List<char> keyCharPresses = new();

		SDL_Event e;
		while ( Sdl2Native.SDL_PollEvent( &e ) != 0 )
		{
			switch ( e.type )
			{
				case SDL_EventType.MouseMotion:
					SDL_MouseMotionEvent mme = Unsafe.Read<SDL_MouseMotionEvent>( &e );
					Snapshot.MouseDelta = new( mme.xrel, mme.yrel );
					Snapshot.MousePosition = new( mme.x, mme.y );

					break;
				case SDL_EventType.MouseButtonDown:
				case SDL_EventType.MouseButtonUp:
					SDL_MouseButtonEvent mbe = Unsafe.Read<SDL_MouseButtonEvent>( &e );
					bool isButtonDown = (mbe.type == SDL_EventType.MouseButtonDown);
					var veldridButton = Veldrid.MouseButton.Left;

					switch ( mbe.button )
					{
						case SDL_MouseButton.Left:
							veldridButton = Veldrid.MouseButton.Left;
							Snapshot.MouseLeft = isButtonDown;
							break;
						case SDL_MouseButton.Right:
							veldridButton = Veldrid.MouseButton.Right;
							Snapshot.MouseRight = isButtonDown;
							break;
						default:
							break;
					}

					var mbeVeldrid = new Veldrid.MouseEvent( veldridButton, isButtonDown );

					veldridMouseEvents.RemoveAll( x => x.MouseButton == veldridButton );
					veldridMouseEvents.Add( mbeVeldrid );
					break;
				case SDL_EventType.MouseWheel:
					SDL_MouseWheelEvent mwe = Unsafe.Read<SDL_MouseWheelEvent>( &e );
					Snapshot.WheelDelta = mwe.y;

					break;
				case SDL_EventType.KeyDown:
				case SDL_EventType.KeyUp:
					SDL_KeyboardEvent kbe = Unsafe.Read<SDL_KeyboardEvent>( &e );
					bool isKeyDown = (kbe.type == SDL_EventType.KeyDown);

					if ( isKeyDown )
					{
						if ( !Snapshot.KeysDown.Any( x => x.sym == kbe.keysym.sym ) )
							Snapshot.KeysDown.Add( kbe.keysym );
					}
					else
					{
						Snapshot.KeysDown.RemoveAll( x => x.sym == kbe.keysym.sym );
					}

					var veldridModifiers = Veldrid.ModifierKeys.None;
					var veldridKey = MapKey( kbe.keysym );
					var kbeVeldrid = new Veldrid.KeyEvent( veldridKey, isKeyDown, veldridModifiers );

					veldridKeyEvents.Add( kbeVeldrid );

					break;

				case SDL_EventType.TextInput:
					SDL_TextInputEvent tie = Unsafe.Read<SDL_TextInputEvent>( &e );

					uint byteCount = 0;
					// Loop until the null terminator is found or the max size is reached.
					while ( byteCount < SDL_TextInputEvent.MaxTextSize && tie.text[byteCount++] != 0 )
					{ }

					if ( byteCount > 1 )
					{
						// We don't want the null terminator.
						byteCount -= 1;
						int charCount = Encoding.UTF8.GetCharCount( tie.text, (int)byteCount );
						char* charsPtr = stackalloc char[charCount];
						Encoding.UTF8.GetChars( tie.text, (int)byteCount, charsPtr, charCount );
						for ( int i = 0; i < charCount; i++ )
						{
							keyCharPresses.Add( charsPtr[i] );
						}
					}

					break;

				case SDL_EventType.WindowEvent:
					SDL_WindowEvent we = Unsafe.Read<SDL_WindowEvent>( &e );
					switch ( we.@event )
					{
						case SDL_WindowEventID.Resized:
						case SDL_WindowEventID.SizeChanged:
							var newSize = new Point2( we.data1, we.data2 );
							Screen.UpdateFrom( newSize );
							Event.Run( Event.Window.ResizedAttribute.Name, newSize );
							break;

						case SDL_WindowEventID.Close:
							// TODO: Unload & destroy everything nicely
							Environment.Exit( 0 );
							break;

						default:
							break;
					}

					break;
			}
		}

		Snapshot.Forward = 0;
		Snapshot.Left = 0;
		Snapshot.Up = 0;

		if ( IsKeyPressed( SDL_Keycode.SDLK_a ) )
			Snapshot.Left -= 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_d ) )
			Snapshot.Left += 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_w ) )
			Snapshot.Forward += 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_s ) )
			Snapshot.Forward -= 1;

		Snapshot.KeyEvents = veldridKeyEvents;
		Snapshot.MouseEvents = veldridMouseEvents;
		Snapshot.KeyCharPresses = keyCharPresses;

		if ( MouseMode == MouseModes.Unlocked )
		{
			Snapshot.Forward = 0;
			Snapshot.Left = 0;
			Snapshot.Up = 0;

			Snapshot.MouseDelta = Vector2.Zero;
			Snapshot.MouseLeft = false;
			Snapshot.MouseRight = false;
		}
	}

	private static bool IsKeyPressed( SDL_Keycode k ) => Snapshot.KeysDown.Select( x => x.sym ).Contains( k );
	private static bool IsKeyPressed( InputButton b ) => IsKeyPressed( ButtonToKeycode( b ) );

	private static bool WasKeyPressed( SDL_Keycode k ) => Snapshot.LastKeysDown.Select( x => x.sym ).Contains( k );
	private static bool WasKeyPressed( InputButton b ) => WasKeyPressed( ButtonToKeycode( b ) );

	/*
	 * TODO: Can we use attributes here? e.g.
	 * 
	 * public enum InputButton
	 * {
	 *     [SDLKey(SDL_Keycode.SDLK_F1]
	 *     ConsoleToggle,
	 *     //...
	 * }
	 */
	public static SDL_Keycode ButtonToKeycode( InputButton button ) => button switch
	{
		InputButton.ConsoleToggle => SDL_Keycode.SDLK_F1,
		InputButton.RotateLeft => SDL_Keycode.SDLK_LEFT,
		InputButton.RotateRight => SDL_Keycode.SDLK_RIGHT,
		InputButton.Jump => SDL_Keycode.SDLK_SPACE,
		InputButton.Sprint => SDL_Keycode.SDLK_LSHIFT,
		InputButton.QuickSwitcher => SDL_Keycode.SDLK_F2,

		_ => SDL_Keycode.SDLK_UNKNOWN
	};

	public static bool Pressed( InputButton button )
	{
		return IsKeyPressed( button )
			&& !WasKeyPressed( button );
	}

	public static bool Down( InputButton button )
	{
		return IsKeyPressed( button );
	}

	public static bool Released( InputButton button )
	{
		return !IsKeyPressed( button )
			&& WasKeyPressed( button );
	}
}
