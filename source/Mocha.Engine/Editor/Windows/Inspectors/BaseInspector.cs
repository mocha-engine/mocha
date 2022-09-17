using ImGuiNET;
using System.Diagnostics;

namespace Mocha.Engine;

public class BaseInspector
{
	private TimeSince timeSinceCopied = 1000;

	public virtual void Draw()
	{
	}

	protected virtual void DrawProperties( string title, (string, string)[] items, string filePath )
	{
		DrawButtons( filePath );
		ImGuiX.Separator();

		ImGui.BeginListBox( "##inspector_table", new( -1, 42 + items.Length * 24 ) );

		ImGuiX.TextBold( title );
		DrawTable( items );

		ImGui.EndListBox();
	}

	protected void DrawButtons( string filePath )
	{
		ImGui.PushStyleColor( ImGuiCol.Button, Colors.Transparent );

		if ( ImGui.Button( $"{FontAwesome.Folder}" ) )
		{
			var args = $"/select,\"{FileSystem.Game.GetFullPath( filePath )}\"";
			Process.Start( "explorer.exe", args );
		}

		ImGui.SameLine();

		var copyPathButtonText = (timeSinceCopied < 3) ? $"{FontAwesome.ClipboardCheck}" : $"{FontAwesome.Clipboard}";

		if ( ImGui.Button( copyPathButtonText ) )
		{
			ImGui.SetClipboardText( filePath.NormalizePath() );
			timeSinceCopied = 0;
		}

		ImGui.PopStyleColor();
	}

	protected void DrawTable( (string, string)[] items )
	{
		if ( ImGui.BeginTable( $"##details", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
		{
			ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 100f );
			ImGui.TableSetupColumn( "Text", ImGuiTableColumnFlags.WidthStretch, 1f );

			for ( int i = 0; i < items.Length; i++ )
			{
				var line = items[i];

				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				ImGui.Text( line.Item1 );
				ImGui.TableNextColumn();
				ImGui.Text( line.Item2 );
			}

			ImGui.EndTable();
		}
	}
}
