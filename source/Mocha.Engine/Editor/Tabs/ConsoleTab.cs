using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.Bug} Debug/Console" )]
internal class ConsoleTab : BaseTab
{
	List<ConsoleItem> items = new();
	string consoleInput = "";

	struct ConsoleItem
	{
		public Vector4 Color { get; set; }
		public string Text { get; set; }

		public ConsoleItem( Vector4 color, string text )
		{
			Color = color;
			Text = text;
		}
	}

	public ConsoleTab()
	{
		Logger.OnLog += ( severity, str, _ ) =>
		{
			var color = severity switch
			{
				Logger.Level.Trace => OneDark.Trace,
				Logger.Level.Info => OneDark.Info,
				Logger.Level.Warning => OneDark.Warning,
				Logger.Level.Error => OneDark.Error,
				_ => OneDark.Info,
			};

			items.Add( new ConsoleItem( color, str ) );
		};

		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( "Console" );

		ImGui.BeginChild( "logs", new System.Numerics.Vector2( 0, -32 ) );
		if ( ImGui.BeginTable( $"##table_logs", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
		{
			ImGui.TableSetupColumn( "Text", ImGuiTableColumnFlags.WidthStretch, 1f );

			for ( int i = 0; i < items.Count; i++ )
			{
				var line = items[i];

				ImGui.TableNextRow();
				ImGui.PushStyleColor( ImGuiCol.Text, line.Color );
				ImGui.TableNextColumn();
				ImGui.Text( line.Text );
				ImGui.PopStyleColor();
			}

			ImGui.EndTable();
		}

		if ( ImGui.GetScrollY() >= ImGui.GetScrollMaxY() )
			ImGui.SetScrollHereY( 1.0f );

		ImGui.EndChild();

		ImGui.SetNextItemWidth( -68 );
		ImGui.InputText( "##console_input", ref consoleInput, 512 );
		ImGui.SameLine();

		if ( ImGui.Button( "Submit" ) )
		{
			Log.Info( $"Console input: '{consoleInput}'" );
			consoleInput = "";
		}

		ImGui.End();
	}
}
