using ImGuiNET;

namespace Mocha.Engine;

[EditorMenu( FontAwesome.Terminal, $"{FontAwesome.Gears} Engine/Console" )]
internal class ConsoleWindow : BaseEditorWindow
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

	public ConsoleWindow()
	{
		Logger.OnLog += ( severity, str, _ ) =>
		{
			var color = severity switch
			{
				Logger.Level.Trace => Colors.LightText,
				Logger.Level.Info => Colors.Blue,
				Logger.Level.Warning => Colors.Orange,
				Logger.Level.Error => Colors.Red,
				_ => Colors.Blue,
			};

			items.Add( new ConsoleItem( color, str ) );
		};

		isVisible = true;
	}

	public override void Draw()
	{
		if ( ImGui.Begin( "Console" ) )
		{
			ImGui.BeginListBox( "##logs", new System.Numerics.Vector2( -1, -32 ) );

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

			ImGui.EndListBox();

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
}
