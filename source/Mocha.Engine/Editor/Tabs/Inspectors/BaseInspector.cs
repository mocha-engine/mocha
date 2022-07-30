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
		ImGui.BeginListBox( "##inspector_table", new( -1, items.Length * 32 ) );

		EditorHelpers.TextBold( title );
		DrawTable( items );

		ImGui.EndListBox();

		EditorHelpers.Separator();
		DrawButtons( filePath );
	}

	protected void DrawButtons( string filePath )
	{
		float width = (ImGui.GetWindowWidth() - 20f) * 0.5f;

		if ( ImGui.Button( $"{FontAwesome.Folder} Open in Explorer", new System.Numerics.Vector2( width, 0 ) ) )
		{
			Process.Start( "explorer.exe", $"/select,{Path.GetFullPath( filePath )}" );
		}

		ImGui.SameLine();

		var copyPathButtonText = (timeSinceCopied < 3) ? $"{FontAwesome.FaceSmileBeam} Copied!" : $"{FontAwesome.Clipboard} Copy Path";

		if ( ImGui.Button( copyPathButtonText, new System.Numerics.Vector2( width, 0 ) ) )
		{
			ImGui.SetClipboardText( filePath.Normalize() );
			timeSinceCopied = 0;
		}
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
