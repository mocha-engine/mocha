using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Editor;

public class BaseInspector
{
	private TimeSince _timeSinceCopied = 1000;

	public virtual void Draw()
	{
	}

	protected virtual void DrawProperties( string title, (string, string)[] items, string filePath )
	{
		DrawButtons( filePath );
		ImGuiX.Separator();

		int itemCount = items.Length + 1; // Add one for header
		float lineHeight = ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().CellPadding.Y;
		float listBoxHeight = itemCount * lineHeight;
		listBoxHeight += 8f; // Header spacing

		if ( ImGui.BeginListBox( "##inspector_table", new Vector2( -1, listBoxHeight ) ) )
		{
			ImGuiX.TextBold( title );
			DrawTable( items );

			ImGui.EndListBox();
		}
	}

	protected void DrawButtons( string filePath )
	{
		ImGui.PushStyleColor( ImGuiCol.Button, Theme.Transparent );

		if ( ImGui.SmallButton( $"{FontAwesome.Folder}" ) )
		{
			var args = $"/select,\"{FileSystem.Game.GetFullPath( filePath )}\"";
			Process.Start( "explorer.exe", args );
		}

		ImGui.SameLine();

		var copyPathButtonText = (_timeSinceCopied < 3) ? $"{FontAwesome.ClipboardCheck}" : $"{FontAwesome.Clipboard}";

		if ( ImGui.SmallButton( copyPathButtonText ) )
		{
			ImGui.SetClipboardText( filePath.NormalizePath() );
			_timeSinceCopied = 0;
		}

		ImGui.PopStyleColor();
	}

	protected static void DrawTable( (string, string)[] items )
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

	/// <summary>
	/// Attempts to get a suitable <see cref="BasePropertyEditor"/> for the given property type.
	/// </summary>
	/// <param name="propertyType">The type to search for a suitable editor with.</param>
	/// <param name="editorType">The property editor type that was found. Null if none found.</param>
	/// <returns>Whether or not a suitable <see cref="BasePropertyEditor"/> was found.</returns>
	protected static bool TryGetPropertyEditorType( Type propertyType, [NotNullWhen( true )] out Type? editorType )
	{
		var propertyEditorAttributeType = typeof( PropertyEditorAttribute<> ).MakeGenericType( propertyType );
		// TODO: Search all assemblies, there could be custom property editors laying around.
		var propertyEditorTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where( type => type.IsAssignableTo( typeof( BasePropertyEditor ) ) )
			.ToList();

		foreach ( var propertyEditorType in propertyEditorTypes )
		{
			if ( propertyEditorType.GetCustomAttribute( propertyEditorAttributeType ) is null )
				continue;

			editorType = propertyEditorType;
			return true;
		}

		editorType = null;
		return false;
	}
}
