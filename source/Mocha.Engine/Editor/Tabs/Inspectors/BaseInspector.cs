using ImGuiNET;
using System.Diagnostics;
using System.Reflection;

namespace Mocha.Engine;

public class BaseInspector
{
	private TimeSince timeSinceCopied = 1000;

	public virtual void Draw()
	{
	}

	protected void DrawButtons( string filePath )
	{
		float width = (ImGui.GetWindowWidth() - 20f) * 0.5f;

		if ( ImGui.Button( $"{FontAwesome.Folder} Open in Explorer", new System.Numerics.Vector2( width, 0 ) ) )
		{
			Process.Start( "explorer.exe", $"/select,{filePath}" );
		}

		ImGui.SameLine();
		if ( ImGui.Button( $"{FontAwesome.FaceSmile} Beep", new System.Numerics.Vector2( width, 0 ) ) )
		{
			Log.Trace( "Boop" );
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

				if ( ImGui.IsItemClicked() )
				{
					ImGui.SetClipboardText( line.Item2 );
					timeSinceCopied = 0;
				}

				if ( ImGui.IsItemHovered() )
				{
					if ( timeSinceCopied < 3 )
						ImGui.SetTooltip( "Copied!" );
					else
						ImGui.SetTooltip( "Click to copy" );
				}
			}

			ImGui.EndTable();
		}
	}
}
