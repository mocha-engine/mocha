using Mocha.UI;

namespace Mocha.Editor;

[Title( "Console" )]
public class ConsoleWindow : EditorWindow
{
	//
	// ImGUI input variables
	//
	private const int MaxInputLength = 512;
	private static string s_currentInput = "";

	/// <summary>
	/// Has the console just changed? If so, set this to true and
	/// we'll scroll to the bottom.
	/// </summary>
	private bool _isDirty = false;

	private static Vector4 LogLevelToColor( LogLevel level ) => level switch
	{
		LogLevel.Trace => Theme.Blue,
		LogLevel.Debug => Theme.Blue,
		LogLevel.Info => Theme.LightGray,
		LogLevel.Warning => Theme.Orange,
		LogLevel.Error => Theme.Red,
		LogLevel.Critical => Theme.Red,
		_ => Theme.LightGray,
	};

	private void DrawOutput()
	{
		if ( !ImGui.BeginChild( "##console_output", new Vector2( -1, -32 ) ) )
			return;

		if ( _isDirty )
		{
			ImGui.SetScrollY( ImGui.GetScrollMaxY() );

			_isDirty = false;
		}

		if ( ImGui.BeginTable( "##console_output_table", 2, ImGuiTableFlags.RowBg ) )
		{
			ImGui.TableSetupColumn( "Time", ImGuiTableColumnFlags.WidthFixed, 128.0f );
			ImGui.TableSetupColumn( "Text", ImGuiTableColumnFlags.WidthStretch, 1.0f );

			foreach ( NativeLogger.LogEntry item in Log.GetHistory() )
			{
				var level = Enum.Parse<LogLevel>( item.level );
				var color = LogLevelToColor( level );

				ImGui.TableNextRow();
				ImGui.TableNextColumn();

				static void ColoredText( string text, Vector4 color )
				{
					ImGui.PushStyleColor( ImGuiCol.Text, color );
					ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, ImGui.GetColorU32( color.ToBackground() ), -1 );
					ImGuiX.TextMonospace( text );
					ImGui.PopStyleColor();
				}

				ColoredText( item.time, color.ToBackground( 0.75f ) );
				ImGui.TableNextColumn();

				ColoredText( item.message, color );
			}

			ImGui.EndTable();
		}

		ImGui.EndChild();
	}

	private void DrawInput()
	{
		ImGui.SetNextItemWidth( -78 );
		bool pressed = ImGui.InputText( "##console_input", ref s_currentInput, MaxInputLength, ImGuiInputTextFlags.EnterReturnsTrue );

		ImGui.SameLine();
		if ( ImGui.Button( "Submit" ) || pressed )
		{
			Log.Info( $"] {s_currentInput}" );

			ConsoleSystem.Run( s_currentInput );

			_isDirty = true;
			s_currentInput = "";
		}
	}

	public override void Draw()
	{
		if ( ImGui.Begin( "Console", ref isVisible ) )
		{
			DrawOutput();
			DrawInput();

			ImGui.End();
		}
	}
}
