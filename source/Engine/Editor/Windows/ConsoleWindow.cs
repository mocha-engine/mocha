namespace Mocha.Editor;

public class ConsoleWindow : EditorWindow
{
	const int MAX_INPUT_LENGTH = 512;
	static string consoleInput = "";

	public override void Draw()
	{
		if ( ImGui.Begin( "Console" ) )
		{
			if ( ImGui.BeginChild( "##console_output" ) )
			{
				ImGui.BeginTable( "##console_output_table", 3, 0 );

				ImGui.TableSetupColumn( "Time", 0, 64.0f );
				ImGui.TableSetupColumn( "Logger", 0, 64.0f );
				ImGui.TableSetupColumn( "Text", 0, 512.0f );

				foreach ( var item in Log.GetHistory() )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.Text( item.time.ToString() );

					ImGui.TableNextColumn();

					ImGui.Text( item.logger.ToString() );

					ImGui.TableNextColumn();

					ImGui.TextWrapped( item.message );
				}

				ImGui.EndTable();
				ImGui.EndChild();
			}

			ImGui.SetNextItemWidth( -60 );
			consoleInput = ImGui.InputText( "##console_input", consoleInput, MAX_INPUT_LENGTH );

			ImGui.SameLine();
			if ( ImGui.Button( "Submit" ) )
			{
				Log.Trace( $"> {consoleInput}" );
				consoleInput = "";
			}
		}

		ImGui.End();
	}
}
