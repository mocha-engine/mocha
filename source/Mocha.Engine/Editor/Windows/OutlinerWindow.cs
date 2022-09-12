﻿using ImGuiNET;
using System.ComponentModel;
using System.Reflection;

namespace Mocha.Engine;

[Icon( FontAwesome.Cubes ), Title( "Outliner" ), Category( "Game" )]
internal class OutlinerTab : BaseEditorWindow
{
	public static OutlinerTab Instance { get; set; }

	public OutlinerTab()
	{
		Instance = this;
		isVisible = true;
	}

	public void SelectItem( string name )
	{
		InspectorWindow.SetSelectedObject( Entity.All.First( x => x.Name == name ) );
	}

	public override void Draw()
	{
		ImGui.Begin( "Outliner" );

		//
		// Hierarchy
		//
		{
			var groupedEntities = Entity.All.GroupBy( x => x.GetType().GetCustomAttribute<CategoryAttribute>() );

			ImGuiX.Title(
				  $"{FontAwesome.Globe} World",
				"This is where all your entities live."
			);

			ImGui.BeginListBox( "##entity_list", new System.Numerics.Vector2( -1, -1 ) );

			foreach ( var group in groupedEntities )
			{
				string icon = FontAwesome.Question;

				// TODO: Get rid of this
				switch ( group.Key?.Category )
				{
					case "Player":
						icon = FontAwesome.User;
						break;
					case "World":
						icon = FontAwesome.Globe;
						break;
				}

				ImGuiX.TextBold( $"{icon} {group.Key?.Category ?? "Uncategorised"}" );

				if ( ImGui.BeginTable( $"##table_entities", 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
				{
					ImGui.TableSetupColumn( "Entity", ImGuiTableColumnFlags.WidthStretch, 1f );

					foreach ( var entity in group )
					{
						ImGui.TableNextRow();
						ImGui.TableNextColumn();

						var str = $"{entity.Name}";

						if ( ImGui.Selectable( str ) )
						{
							SelectItem( str );
						}
					}

					ImGui.EndTable();
				}

				ImGuiX.Separator();
			}

			ImGui.EndListBox();
		}

		ImGui.End();
	}
}