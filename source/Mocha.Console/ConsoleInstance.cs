using ImGuiNET;
using Mocha.Common;

namespace Mocha.Console;
public class ConsoleInstance
{
	RemoteConsoleClient consoleClient;
	List<ConsoleMessage> items = new();

	string consoleInput = "";
	string consoleFilter = "";

	public string Title { get; }

	public ConsoleInstance()
	{
		Title = DateTime.Now.ToString();

		consoleClient = new();
		consoleClient.OnLog += ( consoleMessage ) =>
		{
			items.Add( consoleMessage );
		};
	}

	public void Render()
	{
		if ( ImGui.BeginChild( "##logs_container", new System.Numerics.Vector2( 0, -48 ) ) )
		{
			if ( ImGui.BeginTable( $"##logs", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp, new System.Numerics.Vector2( 0, 0 ) ) )
			{
				ImGui.TableSetupColumn( "Caller", ImGuiTableColumnFlags.WidthFixed, 256f );
				ImGui.TableSetupColumn( "Message", ImGuiTableColumnFlags.WidthStretch, 1f );

				foreach ( var item in items.ToArray() )
				{
					var className = $"[{item.CallingClass}]".PadRight( 32 );
					var message = item.Message;
					var color = item.Color;

					// Basic filtering
					if ( !string.IsNullOrEmpty( consoleFilter ) )
					{
						if ( !message.Contains( consoleFilter ) && !className.Contains( consoleFilter ) )
							continue;
					}

					ImGui.PushStyleColor( ImGuiCol.Text, color );

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.TextWrapped( className );
					ImGui.TableNextColumn();
					ImGui.TextWrapped( message );

					ImGui.PopStyleColor();
				}

				ImGui.EndTable();
			}

			if ( ImGui.GetScrollY() >= ImGui.GetScrollMaxY() )
				ImGui.SetScrollHereY( 1.0f );

			ImGui.EndChild();
		}

		ImGui.SetNextItemWidth( item_width: 256 );
		ImGui.Text( "Filter" );
		ImGui.SameLine();
		ImGui.SetNextItemWidth( -1 );
		ImGui.InputText( "##console_filter", ref consoleFilter, 512 );

		ImGui.SetNextItemWidth( item_width: 256 );
		ImGui.Text( "Command" );
		ImGui.SameLine();
		ImGui.SetNextItemWidth( -64 );
		ImGui.InputText( "##console_input", ref consoleInput, 512 );
		ImGui.SameLine();

		if ( ImGui.Button( "Submit" ) )
		{
			consoleInput = "";
		}
	}
}
