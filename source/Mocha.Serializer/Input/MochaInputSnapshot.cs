using System.Collections;
using Veldrid.Sdl2;

namespace Mocha.Common;

public class MochaInputSnapshot : InputSnapshot
{
	#region "InputSnapshot Interface"
	public IReadOnlyList<KeyEvent>? KeyEvents { get; internal set; }
	public IReadOnlyList<MouseEvent>? MouseEvents { get; internal set; }
	public IReadOnlyList<char>? KeyCharPresses { get; internal set; }
	public System.Numerics.Vector2 MousePosition { get; internal set; }
	public float WheelDelta { get; internal set; }
	public bool IsMouseDown( MouseButton button ) => button switch
	{
		MouseButton.Left => MouseLeft,
		MouseButton.Right => MouseRight,
		_ => false,
	};
	#endregion

	public System.Numerics.Vector2 MouseDelta { get; internal set; }

	public float Forward { get; set; }
	public float Left { get; set; }
	public float Up { get; set; }

	public bool MouseLeft { get; set; }
	public bool MouseRight { get; set; }

	public List<SDL_Keysym> LastKeysDown { get; set; } = new();
	public List<SDL_Keysym> KeysDown { get; set; } = new();

	public override string ToString()
	{
		string str = "";

		foreach ( var property in this.GetType().GetProperties() )
		{
			var val = property.GetValue( this );

			if ( val is IList list )
			{
				str += $"{property.Name}: ";

				int i = 0;
				foreach ( var item in list )
				{
					str += item + ", ";
					i++;

					if ( i > 16 ) // List too long
						break;
				}

				str += "\n";
			}
			else
			{
				str += $"{property.Name}: {property.GetValue( this )}\n";
			}
		}

		return str;
	}
}
