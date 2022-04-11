using ImGuiNET;
using System.Runtime.CompilerServices;
using Veldrid.Sdl2;

namespace Mocha;

public static partial class Input
{
	public static MochaInputSnapshot InputSnapshot { get; private set; }

	public static System.Numerics.Vector2 MouseDelta => InputSnapshot.MouseDelta;
	public static System.Numerics.Vector2 MousePosition => InputSnapshot.MousePosition;

	public static float Forward => InputSnapshot.Forward;
	public static float Left => InputSnapshot.Left;
	public static float Up => InputSnapshot.Up;

	public static bool MouseLeft => InputSnapshot.MouseLeft;
	public static bool MouseRight => InputSnapshot.MouseRight;

	public static unsafe void Update()
	{
		var io = ImGui.GetIO();

		if ( Sdl2Native.SDL_SetRelativeMouseMode( !Editor.Instance.ShouldRender ) != 0 )
			throw new Exception();

		var lastKeysDown = InputSnapshot?.KeysDown.ToList() ?? new();
		var lastKeyEvents = InputSnapshot?.KeyEvents.ToList() ?? new();
		var lastMouseEvents = InputSnapshot?.MouseEvents.ToList() ?? new();
		InputSnapshot ??= new();
		InputSnapshot.KeysDown = lastKeysDown.ToList();
		InputSnapshot.LastKeysDown = lastKeysDown;
		InputSnapshot.MouseDelta = Vector2.Zero;

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
					InputSnapshot.MouseDelta = new( mme.xrel, mme.yrel );
					InputSnapshot.MousePosition = new( mme.x, mme.y );

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
							InputSnapshot.MouseLeft = isButtonDown;
							break;
						case SDL_MouseButton.Right:
							veldridButton = Veldrid.MouseButton.Right;
							InputSnapshot.MouseRight = isButtonDown;
							break;
						default:
							break;
					}

					var mbeVeldrid = new Veldrid.MouseEvent( veldridButton, isButtonDown );

					veldridMouseEvents.RemoveAll( x => x.MouseButton == veldridButton );
					veldridMouseEvents.Add( mbeVeldrid );
					break;
				case SDL_EventType.KeyDown:
				case SDL_EventType.KeyUp:

					SDL_KeyboardEvent kbe = Unsafe.Read<SDL_KeyboardEvent>( &e );
					bool isKeyDown = (kbe.type == SDL_EventType.KeyDown);

					if ( isKeyDown )
					{
						if ( !InputSnapshot.KeysDown.Any( x => x.sym == kbe.keysym.sym ) )
							InputSnapshot.KeysDown.Add( kbe.keysym );
					}
					else
					{
						InputSnapshot.KeysDown.RemoveAll( x => x.sym == kbe.keysym.sym );
					}

					var veldridModifiers = Veldrid.ModifierKeys.None;
					var veldridKey = MapKey( kbe.keysym );
					var kbeVeldrid = new Veldrid.KeyEvent( veldridKey, isKeyDown, veldridModifiers );

					veldridKeyEvents.Add( kbeVeldrid );

					break;

				case SDL_EventType.WindowEvent:
					SDL_WindowEvent we = Unsafe.Read<SDL_WindowEvent>( &e );
					switch ( we.@event )
					{
						case SDL_WindowEventID.Resized:
						case SDL_WindowEventID.SizeChanged:
							var newSize = new Point2( we.data1, we.data2 );
							Screen.UpdateFrom( newSize ); // TODO: Can we hook this up to Event.Window.Resized?
							Event.Run( Event.Window.ResizedAttribute.Name, newSize );
							break;
						default:
							break;
					}

					break;
			}
		}

		InputSnapshot.Forward = 0;
		InputSnapshot.Left = 0;
		InputSnapshot.Up = 0;

		if ( IsKeyPressed( SDL_Keycode.SDLK_a ) )
			InputSnapshot.Left -= 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_d ) )
			InputSnapshot.Left += 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_w ) )
			InputSnapshot.Forward += 1;
		if ( IsKeyPressed( SDL_Keycode.SDLK_s ) )
			InputSnapshot.Forward -= 1;

		InputSnapshot.KeyEvents = veldridKeyEvents;
		InputSnapshot.MouseEvents = veldridMouseEvents;
		InputSnapshot.KeyCharPresses = keyCharPresses;

		if ( Editor.Instance.ShouldRender )
		{
			InputSnapshot.MouseDelta = Vector2.Zero;
			InputSnapshot.MouseLeft = false;
			InputSnapshot.MouseRight = false;
		}
	}

	private static bool IsKeyPressed( SDL_Keycode k ) => InputSnapshot.KeysDown.Select( x => x.sym ).Contains( k );
	private static bool IsKeyPressed( InputButton b ) => IsKeyPressed( ButtonToKeycode( b ) );

	private static bool WasKeyPressed( SDL_Keycode k ) => InputSnapshot.LastKeysDown.Select( x => x.sym ).Contains( k );
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
